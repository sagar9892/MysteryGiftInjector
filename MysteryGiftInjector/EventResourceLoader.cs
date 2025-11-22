using System;
using System.IO;
using System.Reflection;
using MysteryGiftInjector.SaveInjection;

namespace MysteryGiftInjector.Resources
{
    /// <summary>
    /// Manages embedded resources for Pokemon Gen 3 Mystery Gift injection.
    ///
    /// RESOURCE TYPES:
    /// 1. CRC Table (512 bytes) - CRC-16/XMODEM lookup table for checksum calculations
    /// 2. Wonder Card Data (332 bytes) - Mystery Gift ticket templates
    /// 3. GMScript Data (1000 bytes) - Dialog/script data displayed in-game
    ///
    /// All resources are embedded in the executable during compilation and loaded at runtime.
    /// </summary>
    public static class EventResourceLoader
    {
        #region Embedded Resource Names

        // CRC-16 lookup table - shared by all game types
        private const string CRC_TABLE_RESOURCE = "MysteryGiftInjector.Resources.tab.bin";

        // Emerald resources
        private const string EMERALD_AURORA_WONDERCARD = "MysteryGiftInjector.Resources.Emerald_AuroraTicket.bin";
        private const string EMERALD_AURORA_SCRIPT = "MysteryGiftInjector.Resources.Emerald_AuroraTicket_Script.bin";

        // FireRed/LeafGreen resources
        private const string FRLG_AURORA_WONDERCARD = "MysteryGiftInjector.Resources.FRLG_AuroraTicket.bin";
        private const string FRLG_MYSTIC_WONDERCARD = "MysteryGiftInjector.Resources.FRLG_MysticTicket.bin";
        private const string FRLG_SCRIPT = "MysteryGiftInjector.Resources.FRLG_Script.bin";

        #endregion

        #region Resource Sizes

        private const int CRC_TABLE_SIZE = 512;
        private const int WONDER_CARD_SIZE = 332;
        private const int SCRIPT_SIZE = 1000;

        #endregion

        #region Ticket Type Enumeration

        /// <summary>
        /// Available Mystery Gift ticket types.
        ///
        /// Aurora Ticket: Unlocks Birth Island (Deoxys)
        /// Mystic Ticket: Unlocks Navel Rock (Ho-Oh/Lugia) - FR/LG only
        /// </summary>
        public enum TicketType
        {
            AuroraTicket,
            MysticTicket
        }

        #endregion

        #region Public Resource Loading Methods

        /// <summary>
        /// Loads the CRC-16 lookup table used for checksum calculations.
        ///
        /// This table is shared across all game types and enables fast CRC-16/XMODEM
        /// checksum computation for Wonder Card and GMScript data validation.
        /// </summary>
        /// <returns>512-byte CRC-16 lookup table</returns>
        /// <exception cref="FileNotFoundException">If embedded resource is missing</exception>
        /// <exception cref="InvalidDataException">If resource has incorrect size</exception>
        public static byte[] LoadCrcTable()
        {
            return LoadEmbeddedResource(CRC_TABLE_RESOURCE, CRC_TABLE_SIZE);
        }

        /// <summary>
        /// Loads the Wonder Card template for a specific game and ticket type.
        ///
        /// GAME-SPECIFIC TICKETS:
        /// - Emerald: Only Aurora Ticket is available
        /// - FireRed/LeafGreen: Both Aurora and Mystic tickets available
        ///
        /// The Wonder Card contains the event item data that appears in the player's
        /// Mystery Gift menu and unlocks special in-game locations.
        /// </summary>
        /// <param name="gameType">Game version (Emerald or FireRed/LeafGreen)</param>
        /// <param name="ticketType">Ticket type (Aurora or Mystic)</param>
        /// <returns>332-byte Wonder Card template</returns>
        /// <exception cref="ArgumentException">If invalid ticket type for game</exception>
        /// <exception cref="FileNotFoundException">If embedded resource is missing</exception>
        /// <exception cref="InvalidDataException">If resource has incorrect size</exception>
        public static byte[] LoadWonderCard(SaveFileHandler.GameType gameType, TicketType ticketType)
        {
            string resourceName = DetermineWonderCardResource(gameType, ticketType);
            return LoadEmbeddedResource(resourceName, WONDER_CARD_SIZE);
        }

        /// <summary>
        /// Loads the GMScript/dialog data for a specific game type.
        ///
        /// GMScript contains the text and script commands displayed when the player
        /// receives the Mystery Gift. This includes dialog boxes, item names, and
        /// any special event triggers.
        ///
        /// Note: Emerald uses different script data than FireRed/LeafGreen.
        /// </summary>
        /// <param name="gameType">Game version (Emerald or FireRed/LeafGreen)</param>
        /// <returns>1000-byte GMScript/dialog data</returns>
        /// <exception cref="FileNotFoundException">If embedded resource is missing</exception>
        /// <exception cref="InvalidDataException">If resource has incorrect size</exception>
        public static byte[] LoadScript(SaveFileHandler.GameType gameType)
        {
            string resourceName = DetermineScriptResource(gameType);
            return LoadEmbeddedResource(resourceName, SCRIPT_SIZE);
        }

        #endregion

        #region Resource Name Resolution

        /// <summary>
        /// Determines the correct Wonder Card resource name based on game and ticket type.
        /// </summary>
        /// <exception cref="ArgumentException">If invalid ticket type for selected game</exception>
        private static string DetermineWonderCardResource(SaveFileHandler.GameType gameType, TicketType ticketType)
        {
            if (gameType == SaveFileHandler.GameType.Emerald)
            {
                // Emerald version only supports Aurora Ticket
                if (ticketType != TicketType.AuroraTicket)
                    throw new ArgumentException("Emerald only supports Aurora Ticket");

                return EMERALD_AURORA_WONDERCARD;
            }
            else  // FireRedLeafGreen
            {
                // FR/LG supports both ticket types
                switch (ticketType)
                {
                    case TicketType.AuroraTicket:
                        return FRLG_AURORA_WONDERCARD;

                    case TicketType.MysticTicket:
                        return FRLG_MYSTIC_WONDERCARD;

                    default:
                        throw new ArgumentException("Unknown ticket type: " + ticketType);
                }
            }
        }

        /// <summary>
        /// Determines the correct GMScript resource name based on game type.
        /// </summary>
        private static string DetermineScriptResource(SaveFileHandler.GameType gameType)
        {
            if (gameType == SaveFileHandler.GameType.Emerald)
                return EMERALD_AURORA_SCRIPT;
            else
                return FRLG_SCRIPT;
        }

        #endregion

        #region Resource Loading

        /// <summary>
        /// Loads an embedded resource from the assembly and validates its size.
        ///
        /// All resources are embedded during compilation using the EmbeddedResource build action.
        /// This method provides centralized error handling and size validation.
        /// </summary>
        /// <param name="resourceName">Fully qualified resource name</param>
        /// <param name="expectedSize">Expected size in bytes</param>
        /// <returns>Resource data as byte array</returns>
        /// <exception cref="FileNotFoundException">If resource doesn't exist in assembly</exception>
        /// <exception cref="InvalidDataException">If resource size doesn't match expected</exception>
        private static byte[] LoadEmbeddedResource(string resourceName, int expectedSize)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            using (Stream resourceStream = executingAssembly.GetManifestResourceStream(resourceName))
            {
                // Validate resource exists
                if (resourceStream == null)
                {
                    throw new FileNotFoundException("Embedded resource not found: " + resourceName +
                        ". Ensure the file is set as EmbeddedResource in project properties.");
                }

                // Validate resource size matches expected
                if (resourceStream.Length != expectedSize)
                {
                    throw new InvalidDataException(
                        string.Format("Resource {0} has incorrect size. Expected {1} bytes, got {2} bytes.",
                        resourceName, expectedSize, resourceStream.Length));
                }

                // Read resource data
                byte[] resourceData = new byte[expectedSize];
                int bytesRead = resourceStream.Read(resourceData, 0, expectedSize);

                if (bytesRead != expectedSize)
                {
                    throw new IOException(
                        string.Format("Failed to read complete resource. Expected {0} bytes, read {1} bytes.",
                        expectedSize, bytesRead));
                }

                return resourceData;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Generates a CRC-16/XMODEM lookup table programmatically.
        ///
        /// POLYNOMIAL: 0x1021 (CRC-16/XMODEM standard)
        ///
        /// This method can be used to generate the lookup table if the embedded resource
        /// is unavailable. The generated table will be identical to the original.
        ///
        /// NOTE: This is not used in normal operation - the precomputed table is
        /// loaded from embedded resources for performance.
        /// </summary>
        /// <returns>512-byte CRC-16 lookup table</returns>
        public static byte[] GenerateCrcTable()
        {
            byte[] lookupTable = new byte[512];
            const ushort CRC_POLYNOMIAL = 0x1021;  // CRC-16/XMODEM polynomial

            // Generate entry for each possible byte value (0-255)
            for (int byteValue = 0; byteValue < 256; byteValue++)
            {
                ushort crc = (ushort)(byteValue << 8);

                // Process 8 bits
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ CRC_POLYNOMIAL);
                    else
                        crc = (ushort)(crc << 1);
                }

                // Store as little-endian 16-bit value
                lookupTable[byteValue * 2] = (byte)(crc & 0xFF);       // Low byte
                lookupTable[byteValue * 2 + 1] = (byte)(crc >> 8);     // High byte
            }

            return lookupTable;
        }

        #endregion
    }
}
