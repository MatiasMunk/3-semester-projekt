using System.Drawing;

namespace JaTakTilbud.Client.UI;

public static class Theme
{
    // Colors
    public static Color Background = Color.FromArgb(245, 247, 250);
    public static Color Surface = Color.White;
    public static Color Primary = Color.FromArgb(99, 102, 241);
    public static Color PrimaryHover = Color.FromArgb(79, 82, 221);

    public static Color TextPrimary = Color.FromArgb(30, 30, 30);
    public static Color TextSecondary = Color.FromArgb(120, 120, 120);

    public static Color Border = Color.FromArgb(230, 230, 230);

    // Sidebar
    public static Color SidebarBg = Color.FromArgb(24, 24, 27);
    public static Color SidebarItem = Color.FromArgb(39, 39, 42);
    public static Color SidebarActive = Color.FromArgb(99, 102, 241);

    // Fonts
    public static Font TitleFont = new Font("Segoe UI", 16, FontStyle.Bold);
    public static Font BodyFont = new Font("Segoe UI", 10, FontStyle.Regular);
}