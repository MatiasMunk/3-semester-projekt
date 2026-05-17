(() => {
    const shell = document.querySelector('.room-shell');
    if (!shell) return;

    const sessionId = Number(shell.dataset.sessionId);
    const currentUserId = Number(shell.dataset.userId);
    const ownerUserId = Number(shell.dataset.ownerUserId);
    const displayName = shell.dataset.displayName || 'User';

    const status = document.getElementById('roomStatus');
    const grid = document.getElementById('videoGrid');
    const connectBtn = document.getElementById('connectRoomBtn');
    const cameraBtn = document.getElementById('toggleCameraBtn');
    const micBtn = document.getElementById('toggleMicBtn');
    const leaveBtn = document.getElementById('leaveRoomBtn');
    const notificationBar = document.getElementById('notificationBar');
    const usersList = document.getElementById('usersList');
    const chatForm = document.getElementById('chatForm');
    const chatInput = document.getElementById('chatInput');
    const chatLog = document.getElementById('chatLog');

    const popover = document.getElementById('userPopover');
    const popoverName = document.getElementById('popoverName');
    const popoverMeta = document.getElementById('popoverMeta');
    const popoverPmBtn = document.getElementById('popoverPmBtn');
    const popoverFriendBtn = document.getElementById('popoverFriendBtn');

    const pmDrawer = document.getElementById('pmDrawer');
    const pmTitle = document.getElementById('pmTitle');
    const pmMeta = document.getElementById('pmMeta');
    const pmLog = document.getElementById('pmLog');
    const pmForm = document.getElementById('pmForm');
    const pmInput = document.getElementById('pmInput');
    const pmCloseBtn = document.getElementById('pmCloseBtn');

    let room = null;
    let cameraEnabled = false;
    let micEnabled = false;
    let selectedUser = null;
    let activePmUser = null;

    const participants = new Map();
    const pendingFriendRequests = new Map();
    const unreadPmBySender = new Map();
    const friendUserIds = new Set();
    const encoder = new TextEncoder();
    const decoder = new TextDecoder();

    function setStatus(message) {
        status.textContent = message;
    }

    function escapeHtml(value) {
        return String(value ?? '')
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#039;');
    }

    function userIdFromIdentity(identity) {
        const match = String(identity || '').match(/^user-(\d+)$/);
        return match ? Number(match[1]) : null;
    }

    function participantName(participant) {
        return participant?.name || participant?.identity || 'Participant';
    }

    function userFromParticipant(participant) {
        if (!participant) return null;

        const userId = userIdFromIdentity(participant.identity);
        if (!userId) return null;

        return {
            userId,
            identity: participant.identity,
            displayName: participantName(participant)
        };
    }

    function rememberParticipant(participant) {
        const user = userFromParticipant(participant);
        if (user) {
            participants.set(user.userId, user);
            renderUsersList();
        }
        return user;
    }

    function tileId(participantIdentity, source) {
        return `tile-${participantIdentity}-${source}`.replace(/[^a-zA-Z0-9_-]/g, '-');
    }

    function appendChat(author, message, isSystem = false, userId = null) {
        const line = document.createElement('div');
        line.className = 'chat-line';

        if (isSystem) {
            line.innerHTML = `<em>${escapeHtml(message)}</em>`;
        } else if (userId && userId !== currentUserId) {
            line.innerHTML = `<button type="button" class="chat-user" data-user-id="${userId}" data-display-name="${escapeHtml(author)}">${escapeHtml(author)}:</button> ${escapeHtml(message)}`;
        } else {
            line.innerHTML = `<strong>${escapeHtml(author)}:</strong> ${escapeHtml(message)}`;
        }

        chatLog.appendChild(line);
        chatLog.scrollTop = chatLog.scrollHeight;
    }

    function renderUsersList() {
        if (!usersList) return;

        const users = Array.from(participants.values())
            .sort((a, b) => (a.userId === currentUserId ? -1 : b.userId === currentUserId ? 1 : a.displayName.localeCompare(b.displayName)));

        if (!users.length) {
            usersList.innerHTML = '<div class="user-row current"><span class="user-row-name">Connect to see users</span></div>';
            return;
        }

        usersList.innerHTML = users.map(user => {
            const current = user.userId === currentUserId;
            const unread = unreadPmBySender.get(user.userId)?.length || 0;
            const ownerBadge = user.userId === ownerUserId ? '<span class="owner-badge">Owner</span>' : '';
            return `
                <button type="button" class="user-row${current ? ' current' : ''}" data-user-id="${user.userId}" data-display-name="${escapeHtml(user.displayName)}" ${current ? 'disabled' : ''}>
                    <span class="user-row-name">${escapeHtml(user.displayName)}${current ? ' (you)' : ''}${ownerBadge}${unread ? ` · ${unread} new` : ''}</span>
                    <span class="user-row-status">online</span>
                </button>`;
        }).join('');
    }

    function renderNotifications() {
        if (!notificationBar) return;

        const items = [];

        pendingFriendRequests.forEach(request => {
            items.push(`
                <div class="notification-item">
                    Friend request from <button type="button" class="notification-link" data-user-id="${request.requesterUserId}" data-display-name="${escapeHtml(request.requesterUsername)}">${escapeHtml(request.requesterUsername)}</button>
                    <span class="notification-actions">
                        <button type="button" class="notification-action accept" data-friend-request-id="${request.id}" data-action="accept">Accept</button>
                        <button type="button" class="notification-action ignore" data-friend-request-id="${request.id}" data-action="ignore">Ignore</button>
                    </span>
                </div>`);
        });

        unreadPmBySender.forEach((messages, senderUserId) => {
            const latest = messages[0];
            items.push(`<div class="notification-item">${messages.length} new PM${messages.length === 1 ? '' : 's'} from <button type="button" class="notification-link" data-user-id="${senderUserId}" data-display-name="${escapeHtml(latest.senderUsername)}">${escapeHtml(latest.senderUsername)}</button></div>`);
        });

        notificationBar.innerHTML = items.join('');
        notificationBar.classList.toggle('has-items', items.length > 0);
        renderUsersList();
    }

    async function loadNotifications() {
        try {
            const info = await fetchJson(`/Sessions/Notifications?sessionId=${sessionId}`);

            pendingFriendRequests.clear();
            unreadPmBySender.clear();
            friendUserIds.clear();

            (info.friendUserIds || []).forEach(id => friendUserIds.add(Number(id)));

            (info.pendingFriendRequests || []).forEach(request => {
                pendingFriendRequests.set(request.requesterUserId, request);
                if (!participants.has(request.requesterUserId)) {
                    participants.set(request.requesterUserId, {
                        userId: request.requesterUserId,
                        displayName: request.requesterUsername,
                        identity: `user-${request.requesterUserId}`
                    });
                }
            });

            (info.unreadPrivateMessages || []).forEach(message => {
                const list = unreadPmBySender.get(message.senderUserId) || [];
                list.push(message);
                unreadPmBySender.set(message.senderUserId, list);
                if (!participants.has(message.senderUserId)) {
                    participants.set(message.senderUserId, {
                        userId: message.senderUserId,
                        displayName: message.senderUsername,
                        identity: `user-${message.senderUserId}`
                    });
                }
            });

            renderNotifications();
        } catch (error) {
            // Keep room usable even if notification fetch fails.
            console.warn('Could not load notifications', error);
        }
    }

    function attachTrack(track, participant) {
        if (!track || track.kind !== 'video') return;

        const user = rememberParticipant(participant);
        const id = tileId(participant.identity, track.source || 'camera');
        let tile = document.getElementById(id);

        if (!tile) {
            tile = document.createElement('div');
            tile.className = 'video-tile';
            tile.id = id;
            tile.innerHTML = `<span class="video-label">${escapeHtml(participantName(participant))}</span>`;
            grid.appendChild(tile);
        }

        if (user) {
            tile.dataset.userId = String(user.userId);
            tile.dataset.displayName = user.displayName;
        }

        const existingVideo = tile.querySelector('video');
        if (existingVideo) existingVideo.remove();

        const video = track.attach();
        video.autoplay = true;
        video.playsInline = true;
        tile.prepend(video);
    }

    function detachTrack(track) {
        if (!track) return;
        track.detach().forEach(element => element.remove());
    }

    function removeParticipantTiles(participant) {
        const safeIdentity = participant.identity.replace(/[^a-zA-Z0-9_-]/g, '-');
        grid.querySelectorAll(`[id^="tile-${safeIdentity}"]`).forEach(tile => tile.remove());
    }

    function showUserPopover(user, anchor) {
        if (!user || user.userId === currentUserId) return;

        selectedUser = user;
        const isFriend = friendUserIds.has(user.userId);
        popoverName.textContent = user.displayName;
        popoverMeta.textContent = user.userId === ownerUserId
            ? `User #${user.userId} · Room owner${isFriend ? ' · Friend' : ''}`
            : `User #${user.userId}${isFriend ? ' · Friend' : ''}`;
        popoverFriendBtn.textContent = isFriend ? 'Remove friend' : 'Add friend request';
        popoverFriendBtn.classList.toggle('danger', isFriend);
        popover.classList.add('open');

        const rect = anchor.getBoundingClientRect();
        const left = Math.min(rect.left, window.innerWidth - 240);
        const top = Math.min(rect.bottom + 8, window.innerHeight - 160);
        popover.style.left = `${Math.max(12, left)}px`;
        popover.style.top = `${Math.max(12, top)}px`;
    }

    function hideUserPopover() {
        popover.classList.remove('open');
    }

    function isPmDrawerOpen() {
        return pmDrawer.classList.contains('open');
    }

    function closePmDrawer() {
        pmDrawer.classList.remove('open');
        activePmUser = null;
    }

    async function fetchJson(url, options = {}) {
        const response = await fetch(url, {
            headers: { 'Content-Type': 'application/json', ...(options.headers || {}) },
            ...options
        });

        if (!response.ok) throw new Error(await response.text());
        return await response.json();
    }

    function appendPm(message) {
        const line = document.createElement('div');
        const mine = message.senderUserId === currentUserId;
        line.className = `pm-line${mine ? ' mine' : ''}`;
        const time = message.createdAt ? new Date(message.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '';
        line.innerHTML = `${escapeHtml(message.message)}<span class="pm-time">${escapeHtml(mine ? 'You' : message.senderUsername)} ${escapeHtml(time)}</span>`;
        pmLog.appendChild(line);
        pmLog.scrollTop = pmLog.scrollHeight;
    }

    async function openPm(user) {
        activePmUser = user;
        hideUserPopover();
        unreadPmBySender.delete(user.userId);
        renderNotifications();
        pmTitle.textContent = `PM with ${user.displayName}`;
        pmMeta.textContent = `Persistent private chat · User #${user.userId}`;
        pmLog.innerHTML = '<div class="pm-line">Loading conversation...</div>';
        pmDrawer.classList.add('open');
        pmInput.focus();

        try {
            const messages = await fetchJson(`/Sessions/PrivateMessages?sessionId=${sessionId}&otherUserId=${user.userId}`);
            pmLog.innerHTML = '';

            if (!messages.length) {
                pmLog.innerHTML = '<div class="pm-line">No messages yet. Say hi 👋</div>';
                return;
            }

            messages.forEach(appendPm);
            await loadNotifications();
        } catch (error) {
            pmLog.innerHTML = `<div class="pm-line">Could not load PMs: ${escapeHtml(error.message)}</div>`;
        }
    }

    async function respondToFriendRequest(requestId, accept) {
        if (!Number.isFinite(requestId) || requestId <= 0) {
            appendChat('system', 'Refreshing friend requests before responding...', true);
            await loadNotifications();
            return;
        }

        try {
            const updated = await fetchJson(`/Sessions/FriendRequestResponse/${requestId}`, {
                method: 'POST',
                body: JSON.stringify({ accept })
            });

            pendingFriendRequests.delete(updated.requesterUserId);
            if (accept) friendUserIds.add(updated.requesterUserId);
            renderNotifications();
            appendChat('system', accept
                ? `You accepted ${updated.requesterUsername}'s friend request.`
                : `You ignored ${updated.requesterUsername}'s friend request.`, true);

            if (accept && room) {
                const payload = encoder.encode(JSON.stringify({
                    type: 'friend-accepted-notify',
                    fromUserId: currentUserId,
                    fromDisplayName: displayName,
                    toUserId: updated.requesterUserId
                }));
                await room.localParticipant.publishData(payload, { reliable: true });
            }
        } catch (error) {
            appendChat('system', `Could not respond to friend request: ${error.message}`, true);
        }
    }

    async function removeFriend(user) {
        hideUserPopover();

        try {
            const response = await fetch(`/Sessions/Friend/${user.userId}`, { method: 'DELETE' });
            if (!response.ok) throw new Error(await response.text());

            friendUserIds.delete(user.userId);
            renderNotifications();
            appendChat('system', `${user.displayName} removed from friends.`, true);

            if (room) {
                const payload = encoder.encode(JSON.stringify({
                    type: 'friend-removed-notify',
                    fromUserId: currentUserId,
                    fromDisplayName: displayName,
                    toUserId: user.userId
                }));
                await room.localParticipant.publishData(payload, { reliable: true });
            }
        } catch (error) {
            appendChat('system', `Could not remove friend: ${error.message}`, true);
        }
    }

    async function sendFriendRequest(user) {
        hideUserPopover();

        if (friendUserIds.has(user.userId)) {
            await removeFriend(user);
            return;
        }

        try {
            const request = await fetchJson('/Sessions/FriendRequest', {
                method: 'POST',
                body: JSON.stringify({ receiverUserId: user.userId })
            });

            const becameFriends = request?.status === 'accepted';

            if (becameFriends) {
                friendUserIds.add(user.userId);
                pendingFriendRequests.delete(user.userId);
                renderNotifications();
                appendChat('system', `You are now friends with ${user.displayName}.`, true);
            } else {
                appendChat('system', `Friend request sent to ${user.displayName}.`, true);
            }

            if (room) {
                const payload = encoder.encode(JSON.stringify({
                    type: becameFriends ? 'friend-accepted-notify' : 'friend-request-notify',
                    friendRequestId: request?.id,
                    fromUserId: currentUserId,
                    fromDisplayName: displayName,
                    toUserId: user.userId
                }));
                await room.localParticipant.publishData(payload, { reliable: true });
            }
        } catch (error) {
            appendChat('system', `Could not send friend request: ${error.message}`, true);
        }
    }

    async function connectRoom() {
        if (!window.LivekitClient) {
            setStatus('LiveKit client script did not load. Check internet/CDN access.');
            return;
        }

        try {
            setStatus('Requesting LiveKit token...');
            const response = await fetch(`/Sessions/LiveKitToken/${sessionId}`, { method: 'POST' });
            if (!response.ok) throw new Error(await response.text());
            const info = await response.json();

            room = new LivekitClient.Room({
                adaptiveStream: true,
                dynacast: true
            });

            room
                .on(LivekitClient.RoomEvent.TrackSubscribed, (track, publication, participant) => {
                    attachTrack(track, participant);
                })
                .on(LivekitClient.RoomEvent.TrackUnsubscribed, track => {
                    detachTrack(track);
                })
                .on(LivekitClient.RoomEvent.ParticipantConnected, participant => {
                    rememberParticipant(participant);
                    appendChat('system', `${participantName(participant)} joined the room`, true);
                })
                .on(LivekitClient.RoomEvent.ParticipantDisconnected, participant => {
                    removeParticipantTiles(participant);
                    const user = userFromParticipant(participant);
                    if (user) participants.delete(user.userId);
                    appendChat('system', `${participantName(participant)} left the room`, true);
                    renderUsersList();
                })
                .on(LivekitClient.RoomEvent.DataReceived, (payload, participant) => {
                    const sender = rememberParticipant(participant);

                    try {
                        const data = JSON.parse(decoder.decode(payload));

                        if (data.type === 'chat') {
                            appendChat(sender?.displayName || participantName(participant), data.message, false, data.senderUserId || sender?.userId);
                        }

                        if (data.type === 'pm-notify' && data.toUserId === currentUserId) {
                            if (activePmUser?.userId === data.fromUserId && isPmDrawerOpen()) {
                                fetchJson(`/Sessions/PrivateMessages?sessionId=${sessionId}&otherUserId=${data.fromUserId}`)
                                    .then(messages => {
                                        pmLog.innerHTML = '';
                                        messages.forEach(appendPm);
                                        return loadNotifications();
                                    })
                                    .catch(() => {});
                            } else {
                                const existing = unreadPmBySender.get(data.fromUserId) || [];
                                existing.unshift({ senderUserId: data.fromUserId, senderUsername: data.fromDisplayName, message: 'New private message' });
                                unreadPmBySender.set(data.fromUserId, existing);
                                renderNotifications();
                            }
                        }

                        if (data.type === 'friend-request-notify' && data.toUserId === currentUserId) {
                            pendingFriendRequests.set(data.fromUserId, {
                                id: data.friendRequestId,
                                requesterUserId: data.fromUserId,
                                requesterUsername: data.fromDisplayName
                            });
                            renderNotifications();
                        }

                        if (data.type === 'friend-accepted-notify' && data.toUserId === currentUserId) {
                            friendUserIds.add(data.fromUserId);
                            pendingFriendRequests.delete(data.fromUserId);
                            renderNotifications();
                            appendChat('system', `${data.fromDisplayName} accepted your friend request.`, true);
                        }

                        if (data.type === 'friend-removed-notify' && data.toUserId === currentUserId) {
                            friendUserIds.delete(data.fromUserId);
                            pendingFriendRequests.delete(data.fromUserId);
                            renderNotifications();
                            hideUserPopover();
                            appendChat('system', `${data.fromDisplayName} removed you as a friend.`, true);
                        }
                    } catch {
                        appendChat(participantName(participant), decoder.decode(payload), false, sender?.userId);
                    }
                })
                .on(LivekitClient.RoomEvent.Disconnected, () => {
                    setStatus('Disconnected.');
                    cameraBtn.disabled = true;
                    micBtn.disabled = true;
                    leaveBtn.disabled = true;
                    connectBtn.disabled = false;
                    grid.innerHTML = '';
                    participants.clear();
                    renderUsersList();
                });

            await room.connect(info.url, info.token);

            participants.set(currentUserId, {
                userId: currentUserId,
                identity: info.identity,
                displayName: info.displayName
            });

            room.remoteParticipants?.forEach(participant => rememberParticipant(participant));
            renderUsersList();
            await loadNotifications();

            setStatus(`Connected to ${info.room} as ${info.displayName}.`);
            appendChat('system', `Connected to #general as ${info.displayName}`, true);

            connectBtn.disabled = true;
            cameraBtn.disabled = false;
            micBtn.disabled = false;
            leaveBtn.disabled = false;
        } catch (error) {
            setStatus(`Could not connect: ${error.message}`);
        }
    }

    async function toggleCamera() {
        if (!room) return;
        cameraEnabled = !cameraEnabled;
        await room.localParticipant.setCameraEnabled(cameraEnabled);
        cameraBtn.textContent = cameraEnabled ? 'Camera off' : 'Camera on';

        const publication = Array.from(room.localParticipant.videoTrackPublications.values())[0];
        if (publication?.track && cameraEnabled) {
            attachTrack(publication.track, room.localParticipant);
        }
    }

    async function toggleMic() {
        if (!room) return;
        micEnabled = !micEnabled;
        await room.localParticipant.setMicrophoneEnabled(micEnabled);
        micBtn.textContent = micEnabled ? 'Mic off' : 'Mic on';
    }

    function leaveRoom() {
        if (room) room.disconnect();
    }

    async function sendChat(event) {
        event.preventDefault();
        if (!room) return;

        const message = chatInput.value.trim();
        if (!message) return;

        const payload = encoder.encode(JSON.stringify({
            type: 'chat',
            channel: 'general',
            senderUserId: currentUserId,
            message
        }));

        await room.localParticipant.publishData(payload, { reliable: true });
        appendChat(displayName, message, false, currentUserId);
        chatInput.value = '';
    }

    async function sendPm(event) {
        event.preventDefault();
        if (!activePmUser) return;

        const message = pmInput.value.trim();
        if (!message) return;

        try {
            const saved = await fetchJson('/Sessions/PrivateMessages', {
                method: 'POST',
                body: JSON.stringify({
                    sessionId,
                    receiverUserId: activePmUser.userId,
                    message
                })
            });

            const empty = pmLog.querySelector('.pm-line');
            if (empty && empty.textContent.includes('No messages yet')) pmLog.innerHTML = '';

            appendPm(saved);
            pmInput.value = '';

            if (room) {
                const payload = encoder.encode(JSON.stringify({
                    type: 'pm-notify',
                    fromUserId: currentUserId,
                    fromDisplayName: displayName,
                    toUserId: activePmUser.userId
                }));
                await room.localParticipant.publishData(payload, { reliable: true });
            }
        } catch (error) {
            appendChat('system', `Could not send PM: ${error.message}`, true);
        }
    }

    connectBtn.addEventListener('click', connectRoom);
    cameraBtn.addEventListener('click', toggleCamera);
    micBtn.addEventListener('click', toggleMic);
    leaveBtn.addEventListener('click', leaveRoom);
    chatForm.addEventListener('submit', sendChat);
    pmForm.addEventListener('submit', sendPm);
    pmCloseBtn.addEventListener('click', closePmDrawer);

    popoverPmBtn.addEventListener('click', () => selectedUser && openPm(selectedUser));
    popoverFriendBtn.addEventListener('click', () => selectedUser && (friendUserIds.has(selectedUser.userId) ? removeFriend(selectedUser) : sendFriendRequest(selectedUser)));

    grid.addEventListener('click', event => {
        const tile = event.target.closest('.video-tile');
        if (!tile?.dataset.userId) return;

        showUserPopover({
            userId: Number(tile.dataset.userId),
            displayName: tile.dataset.displayName || `User ${tile.dataset.userId}`
        }, tile);
    });

    chatLog.addEventListener('click', event => {
        const userButton = event.target.closest('.chat-user');
        if (!userButton) return;

        showUserPopover({
            userId: Number(userButton.dataset.userId),
            displayName: userButton.dataset.displayName || `User ${userButton.dataset.userId}`
        }, userButton);
    });

    usersList?.addEventListener('click', event => {
        const row = event.target.closest('.user-row');
        if (!row?.dataset.userId) return;

        showUserPopover({
            userId: Number(row.dataset.userId),
            displayName: row.dataset.displayName || `User ${row.dataset.userId}`
        }, row);
    });

    notificationBar?.addEventListener('click', event => {
        const action = event.target.closest('.notification-action');
        if (action) {
            respondToFriendRequest(Number(action.dataset.friendRequestId), action.dataset.action === 'accept');
            return;
        }

        const link = event.target.closest('.notification-link');
        if (!link) return;

        const user = {
            userId: Number(link.dataset.userId),
            displayName: link.dataset.displayName || `User ${link.dataset.userId}`
        };

        if (unreadPmBySender.has(user.userId)) {
            openPm(user);
        } else {
            showUserPopover(user, link);
        }
    });

    document.addEventListener('click', event => {
        if (popover.contains(event.target)) return;
        if (event.target.closest('.video-tile')) return;
        if (event.target.closest('.chat-user')) return;
        if (event.target.closest('.user-row')) return;
        if (event.target.closest('.notification-link')) return;
        hideUserPopover();
    });

    renderUsersList();
})();
