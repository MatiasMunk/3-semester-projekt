using StudyGroups.Client.UI.Controls;

namespace StudyGroups.Client.Views;

partial class CreateStudySessionView
{
    private System.ComponentModel.IContainer components = null;
    private UnderlineTextBox txtTitle;
    private UnderlineTextBox txtSubject;
    private UnderlineTextBox txtLocation;
    private UnderlineTextBox txtMaxParticipants;
    private DateTimePicker dtStart;
    private Label lblPreviewTitle;
    private Label lblPreviewSubject;
    private Label lblPreviewDetails;
    private ModernButton btnCreate;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        txtTitle = new UnderlineTextBox();
        txtSubject = new UnderlineTextBox();
        txtLocation = new UnderlineTextBox();
        txtMaxParticipants = new UnderlineTextBox();
        txtMaxParticipants.IsNumeric = true;
        dtStart = new DateTimePicker();
        lblPreviewTitle = new Label();
        lblPreviewSubject = new Label();
        lblPreviewDetails = new Label();
        btnCreate = new ModernButton();
        SuspendLayout();
        Name = "CreateStudySessionView";
        Size = new System.Drawing.Size(1000, 650);
        ResumeLayout(false);
    }
}