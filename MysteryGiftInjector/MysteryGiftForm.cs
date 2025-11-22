using System;
using System.Windows.Forms;
using MysteryGiftInjector.Resources;
using MysteryGiftInjector.SaveInjection;

namespace MysteryGiftInjector
{
    /// <summary>
    /// Main application form for Pokemon Mystery Gift Injector.
    ///
    /// UI FLOW:
    /// 1. User selects game version (Emerald or Fire Red/Leaf Green)
    /// 2. Appropriate ticket options are displayed
    /// 3. User selects specific Mystery Gift ticket
    /// 4. User drags save file onto window
    /// 5. Injection is performed and result is displayed
    ///
    /// DRAG & DROP BEHAVIOR:
    /// Drag & drop is disabled until a ticket is selected to prevent premature file drops.
    /// </summary>
    public partial class MysteryGiftForm : Form
    {
        #region Constructor and Initialization

        public MysteryGiftForm()
        {
            InitializeComponent();

            InitializeDragAndDrop();
            AttachEventHandlers();
            SetInitialUIState();
        }

        /// <summary>
        /// Configures drag and drop functionality.
        /// Initially disabled - enabled only after ticket selection.
        /// </summary>
        private void InitializeDragAndDrop()
        {
            this.AllowDrop = false;  // Disabled until ticket selected
            this.DragEnter += OnDragEnter;
            this.DragDrop += OnDragDrop;
        }

        /// <summary>
        /// Attaches event handlers to UI controls.
        /// </summary>
        private void AttachEventHandlers()
        {
            // Game version selection
            radioTopOptionA.CheckedChanged += OnGameVersionChanged;
            radioTopOptionB.CheckedChanged += OnGameVersionChanged;

            // Ticket selection
            radioGroup1Opt1.CheckedChanged += OnTicketSelected;
            radioGroup2Opt1.CheckedChanged += OnTicketSelected;
            radioGroup2Opt2.CheckedChanged += OnTicketSelected;
        }

        /// <summary>
        /// Sets the initial state of UI controls.
        /// All controls start invisible in designer and are selectively shown here.
        /// </summary>
        private void SetInitialUIState()
        {
            // Show game version selection
            radioTopOptionA.Visible = true;
            radioTopOptionB.Visible = true;
            groupBox3.Visible = true;  // "Select Version" group
            labelStatus.Visible = true;

            // Hide ticket selection groups (shown after game selection)
            groupBox1.Visible = false;  // Emerald tickets
            groupBox2.Visible = false;  // FR/LG tickets

            // Clear status message
            labelStatus.Text = "";
        }

        #endregion

        #region Game Version Selection

        /// <summary>
        /// Handles game version selection (Emerald or Fire Red/Leaf Green).
        ///
        /// BEHAVIOR:
        /// - Hides version selection group
        /// - Shows appropriate ticket selection group
        /// - Clears any existing ticket selection
        /// - Disables drag & drop until new ticket is selected
        /// </summary>
        private void OnGameVersionChanged(object sender, EventArgs e)
        {
            if (radioTopOptionA.Checked)
            {
                ShowEmeraldTicketOptions();
            }
            else if (radioTopOptionB.Checked)
            {
                ShowFireRedLeafGreenTicketOptions();
            }
        }

        /// <summary>
        /// Displays ticket options for Emerald version.
        /// </summary>
        private void ShowEmeraldTicketOptions()
        {
            // Hide version selection, show Emerald tickets
            groupBox3.Visible = false;
            groupBox1.Visible = true;
            groupBox2.Visible = false;

            // Make Emerald ticket option visible
            radioGroup1Opt1.Visible = true;

            // Clear all ticket selections
            ClearTicketSelections();

            // Disable drag & drop until ticket selected
            this.AllowDrop = false;

            // Clear status message
            labelStatus.Text = "";
        }

        /// <summary>
        /// Displays ticket options for Fire Red / Leaf Green version.
        /// </summary>
        private void ShowFireRedLeafGreenTicketOptions()
        {
            // Hide version selection, show FR/LG tickets
            groupBox3.Visible = false;
            groupBox1.Visible = false;
            groupBox2.Visible = true;

            // Make FR/LG ticket options visible
            radioGroup2Opt1.Visible = true;
            radioGroup2Opt2.Visible = true;

            // Clear all ticket selections
            ClearTicketSelections();

            // Disable drag & drop until ticket selected
            this.AllowDrop = false;

            // Clear status message
            labelStatus.Text = "";
        }

        /// <summary>
        /// Clears all ticket radio button selections.
        /// </summary>
        private void ClearTicketSelections()
        {
            radioGroup1Opt1.Checked = false;  // Emerald Aurora
            radioGroup2Opt1.Checked = false;  // FR/LG Mystic
            radioGroup2Opt2.Checked = false;  // FR/LG Aurora
        }

        #endregion

        #region Ticket Selection

        /// <summary>
        /// Handles ticket selection by user.
        ///
        /// BEHAVIOR:
        /// - Updates status label with selection confirmation
        /// - Enables drag & drop functionality
        /// </summary>
        private void OnTicketSelected(object sender, EventArgs e)
        {
            RadioButton selectedTicket = sender as RadioButton;

            // Only process when radio button is being checked (not unchecked)
            if (selectedTicket != null && selectedTicket.Checked)
            {
                UpdateStatusLabel(selectedTicket.Text);
                EnableDragAndDrop();
            }
        }

        /// <summary>
        /// Updates the status label with ticket selection information.
        /// </summary>
        /// <param name="ticketName">Name of the selected ticket</param>
        private void UpdateStatusLabel(string ticketName)
        {
            string gameLabel = GetSelectedGameLabel();
            labelStatus.Text = "You have selected " + gameLabel + " - " + ticketName +
                             ". Please drag your save onto the window.";
        }

        /// <summary>
        /// Gets a display label for the currently selected game version.
        /// </summary>
        /// <returns>Game label string ("Emerald" or "FR / LG")</returns>
        private string GetSelectedGameLabel()
        {
            if (radioTopOptionA.Checked)
                return "Emerald";
            else if (radioTopOptionB.Checked)
                return "FR / LG";
            else
                return "";
        }

        /// <summary>
        /// Enables drag and drop functionality after ticket selection.
        /// </summary>
        private void EnableDragAndDrop()
        {
            this.AllowDrop = true;
        }

        #endregion

        #region Drag and Drop Handling

        /// <summary>
        /// Handles drag enter event - validates that files are being dragged.
        /// </summary>
        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// Handles file drop event - processes the save file for injection.
        ///
        /// VALIDATION:
        /// 1. Ensures game version is selected
        /// 2. Ensures ticket type is selected
        /// 3. Loads required resources
        /// 4. Performs injection
        /// 5. Displays result
        /// </summary>
        private void OnDragDrop(object sender, DragEventArgs e)
        {
            // Validate file drop data
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (droppedFiles == null || droppedFiles.Length == 0)
                return;

            string saveFilePath = droppedFiles[0];

            try
            {
                // Validate game version selection
                SaveFileHandler.GameType selectedGameType = GetSelectedGameType();
                if (!IsGameTypeSelected())
                {
                    ShowWarning("Please select a game type (Emerald or Fire Red / Leaf Green) first.",
                              "No Game Selected");
                    return;
                }

                // Validate ticket selection
                EventResourceLoader.TicketType selectedTicketType;
                if (!TryGetSelectedTicketType(selectedGameType, out selectedTicketType))
                {
                    ShowWarning("Please select a ticket type first.", "No Ticket Selected");
                    return;
                }

                // Perform injection
                PerformInjection(saveFilePath, selectedGameType, selectedTicketType);
            }
            catch (Exception ex)
            {
                ShowError("Error: " + ex.Message, "Exception");
            }
        }

        #endregion

        #region Game Type and Ticket Type Selection

        /// <summary>
        /// Gets the currently selected game type.
        /// </summary>
        /// <returns>Selected game type enum value</returns>
        private SaveFileHandler.GameType GetSelectedGameType()
        {
            if (radioTopOptionA.Checked)
                return SaveFileHandler.GameType.Emerald;
            else
                return SaveFileHandler.GameType.FireRedLeafGreen;
        }

        /// <summary>
        /// Checks if a game type has been selected.
        /// </summary>
        /// <returns>True if game type is selected, false otherwise</returns>
        private bool IsGameTypeSelected()
        {
            return radioTopOptionA.Checked || radioTopOptionB.Checked;
        }

        /// <summary>
        /// Attempts to get the currently selected ticket type.
        /// </summary>
        /// <param name="gameType">The selected game type</param>
        /// <param name="ticketType">Output parameter for selected ticket type</param>
        /// <returns>True if a ticket is selected, false otherwise</returns>
        private bool TryGetSelectedTicketType(SaveFileHandler.GameType gameType,
                                              out EventResourceLoader.TicketType ticketType)
        {
            ticketType = EventResourceLoader.TicketType.AuroraTicket;  // Default value

            if (gameType == SaveFileHandler.GameType.Emerald)
            {
                // Emerald only has Aurora Ticket
                if (radioGroup1Opt1.Checked)
                {
                    ticketType = EventResourceLoader.TicketType.AuroraTicket;
                    return true;
                }
            }
            else  // FireRedLeafGreen
            {
                // FR/LG has Mystic and Aurora tickets
                if (radioGroup2Opt1.Checked)
                {
                    ticketType = EventResourceLoader.TicketType.MysticTicket;
                    return true;
                }
                if (radioGroup2Opt2.Checked)
                {
                    ticketType = EventResourceLoader.TicketType.AuroraTicket;
                    return true;
                }
            }

            return false;  // No ticket selected
        }

        #endregion

        #region Injection Process

        /// <summary>
        /// Performs the Mystery Gift injection process.
        ///
        /// STEPS:
        /// 1. Load embedded resources (CRC table, Wonder Card data, script data)
        /// 2. Create SaveFileProcessor with resources
        /// 3. Execute injection
        /// 4. Display result to user
        /// </summary>
        private void PerformInjection(string saveFilePath, SaveFileHandler.GameType gameType,
                                     EventResourceLoader.TicketType ticketType)
        {
            // Load embedded resources
            byte[] crcTable = EventResourceLoader.LoadCrcTable();
            byte[] wonderCard = EventResourceLoader.LoadWonderCard(gameType, ticketType);
            byte[] script = EventResourceLoader.LoadScript(gameType);

            // Get executable directory for output file
            string executableDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

            // Create processor and perform injection
            SaveFileHandler processor = new SaveFileHandler(crcTable, wonderCard, script);
            string result = processor.InjectWonderCard(saveFilePath, gameType, executableDirectory);

            // Display result (all messages shown with no icon)
            ShowMessage(result);
        }

        #endregion

        #region UI Message Display

        /// <summary>
        /// Displays a message to the user.
        /// All messages are shown without icons to match original behavior.
        /// </summary>
        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        /// <summary>
        /// Displays a warning message to the user.
        /// </summary>
        private void ShowWarning(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Displays an error message to the user.
        /// </summary>
        private void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion
    }
}
