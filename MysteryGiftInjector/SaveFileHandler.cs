using System;
using System.IO;

namespace MysteryGiftInjector.SaveInjection
{
    /// <summary>
    /// Core engine for processing Pokemon Generation 3 save files and injecting Mystery Gift Wonder Card data.
    ///
    /// SAVE FILE ARCHITECTURE:
    /// Pokemon Gen 3 games use a dual-slot save system where two copies of save data exist simultaneously.
    /// Each slot contains 14 blocks of 4KB (57KB total per slot). The game writes to alternating slots,
    /// incrementing a counter each time. The slot with the higher counter is the active save.
    ///
    /// MYSTERY GIFT DATA LOCATION:
    /// Block 0x04 (Wonder Card block) stores Mystery Gift data at game-specific offsets:
    /// - Emerald: offset 1388 (0x56C)
    /// - FireRed/LeafGreen: offset 1120 (0x460)
    ///
    /// This processor performs non-destructive injection, reading the original save and outputting
    /// a modified copy to "Pokemon Injection.sav" in the executable directory.
    /// </summary>
    public class SaveFileHandler
    {
        #region Save File Structure Constants

        // Dual-slot save architecture
        private const int SLOT_SIZE = 57343;                        // 0xDFFF bytes per slot
        private const int SLOT_1_OFFSET = 0;                        // Slot 1 at file start
        private const int SLOT_2_OFFSET = 57344;                    // 0xE000 - Slot 2 immediately after
        private const int SLOT_1_COUNTER_OFFSET = 4092;             // 0x0FFC - Save counter for slot 1
        private const int SLOT_2_COUNTER_OFFSET = 61436;            // 0xE000 + 0x0FFC

        // Block structure
        private const int BLOCK_SIZE = 4096;                        // 4KB per block
        private const int BLOCK_MARKER_OFFSET = 4084;               // 0xFF4 - Block type identifier offset
        private const int BLOCK_FOOTER_CHECKSUM_OFFSET = 4086;      // 0xFF6 - Last 2 bytes of block data
        private const int MAX_BLOCK_INDEX = 14;                     // Blocks 0-14 (15 total)

        // Block markers for identification
        private const byte GAME_TYPE_DETECTION_BLOCK_MARKER = 0x00; // Block containing game type data
        private const byte WONDER_CARD_BLOCK_MARKER = 0x04;         // Block containing Mystery Gift data

        // Game type detection
        private const int GAME_TYPE_MARKER_OFFSET = 172;            // Offset within block 0x00
        private const int GAME_TYPE_VALUE_RUBY_SAPPHIRE = 0;        // Not supported
        private const int GAME_TYPE_VALUE_FIRERED_LEAFGREEN = 1;    // FR/LG
        // All other values indicate Emerald

        // Wonder Card structure offsets (game-specific base locations)
        private const int EMERALD_WONDER_CARD_BASE = 1388;          // 0x56C
        private const int FRLG_WONDER_CARD_BASE = 1120;             // 0x460

        #endregion

        #region Wonder Card Data Structure Offsets

        // Wonder Card structure (relative to game-specific base offset)
        private const int WONDER_CARD_CRC16_OFFSET = 0;             // +0: CRC-16 checksum
        private const int WONDER_CARD_PAYLOAD_OFFSET = 4;           // +4: 332-byte Wonder Card data
        private const int WONDER_CARD_TERMINATOR_OFFSET = 346;      // +346: 0xFF 0xFF marker

        // GMScript structure (follows Wonder Card in same block)
        private const int GMSCRIPT_CRC16_OFFSET = 828;              // +828: CRC-16 checksum
        private const int GMSCRIPT_PAYLOAD_OFFSET = 832;            // +832: 1000-byte script data

        #endregion

        #region Resource Data Size Constants

        private const int CRC_TABLE_SIZE = 512;
        private const int WONDER_CARD_DATA_SIZE = 332;
        private const int GMSCRIPT_DATA_SIZE = 1000;
        private const int SAVE_COUNTER_SIZE = 4;

        #endregion

        #region Checksum Algorithm Constants

        private const int CRC16_INITIAL_VALUE = 0x1121;             // CRC-16/XMODEM seed
        private const int BLOCK_CHECKSUM_INTEGER_COUNT = 962;       // 962 integers × 4 bytes = 3848 bytes
        private const byte EMPTY_FLASH_BYTE = 0xFF;                 // Erased flash memory value

        #endregion

        #region Game Type Enumeration

        /// <summary>
        /// Supported Pokemon Gen 3 game types.
        /// Ruby and Sapphire are not supported as they lack Mystery Gift/Wonder Card functionality.
        /// </summary>
        public enum GameType
        {
            Emerald,
            FireRedLeafGreen
        }

        #endregion

        #region Private Fields

        private readonly byte[] crcTable;       // 512-byte CRC-16/XMODEM lookup table
        private readonly byte[] wonderCardData; // 332-byte Wonder Card payload
        private readonly byte[] scriptData;     // 1000-byte GMScript dialog data

        #endregion

        #region Constructor and Validation

        /// <summary>
        /// Initializes the save file processor with embedded resource data.
        /// </summary>
        /// <param name="crcTable">512-byte CRC-16 lookup table</param>
        /// <param name="wonderCardData">332-byte Wonder Card payload</param>
        /// <param name="scriptData">1000-byte GMScript dialog data</param>
        /// <exception cref="ArgumentException">If any resource size is incorrect</exception>
        public SaveFileHandler(byte[] crcTable, byte[] wonderCardData, byte[] scriptData)
        {
            ValidateResourceSize(crcTable, CRC_TABLE_SIZE, "CRC table");
            ValidateResourceSize(wonderCardData, WONDER_CARD_DATA_SIZE, "Wonder Card data");
            ValidateResourceSize(scriptData, GMSCRIPT_DATA_SIZE, "Script data");

            this.crcTable = crcTable;
            this.wonderCardData = wonderCardData;
            this.scriptData = scriptData;
        }

        /// <summary>
        /// Validates resource array size matches expected value.
        /// </summary>
        private void ValidateResourceSize(byte[] resource, int expectedSize, string resourceName)
        {
            if (resource == null || resource.Length != expectedSize)
                throw new ArgumentException(resourceName + " must be exactly " + expectedSize + " bytes");
        }

        #endregion

        #region Main Injection Method

        /// <summary>
        /// Injects Mystery Gift data into a Pokemon Gen 3 save file.
        ///
        /// PROCESS:
        /// 1. Load save file into memory
        /// 2. Determine active save slot (highest counter value)
        /// 3. Locate Wonder Card block (0x04) and game detection block (0x00)
        /// 4. Validate save file game type matches selected Mystery Gift
        /// 5. Inject Wonder Card + GMScript data with CRC-16 checksums
        /// 6. Update block footer checksum
        /// 7. Write output to "Pokemon Injection.sav"
        /// </summary>
        /// <param name="saveFilePath">Path to source .sav file</param>
        /// <param name="selectedGame">Game type of Mystery Gift being injected</param>
        /// <param name="executableDirectory">Directory for output file</param>
        /// <returns>Success message or error description</returns>
        public string InjectWonderCard(string saveFilePath, GameType selectedGame, string executableDirectory)
        {
            try
            {
                // Validate file exists
                if (!File.Exists(saveFilePath))
                    return "Error: Save file not found.";

                // Load entire save into memory
                byte[] saveData = File.ReadAllBytes(saveFilePath);

                // Process save data in memory
                using (MemoryStream stream = new MemoryStream(saveData))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    // Determine active slot
                    int activeSlot = DetermineActiveSlot(reader);
                    if (activeSlot == -1)
                        return "Error: No valid save data found in file.";

                    // Locate required blocks
                    int wonderCardBlock = FindWonderCardBlock(reader, activeSlot);
                    if (wonderCardBlock == -1)
                        return "Error: Could not locate Wonder Card block.";

                    int gameDetectionBlock = FindGameDetectionBlock(reader, activeSlot);
                    if (gameDetectionBlock == -1)
                        return "Error: Could not locate game detection block.";

                    // Validate game compatibility
                    string compatibilityError = ValidateGameCompatibility(reader, activeSlot,
                        gameDetectionBlock, selectedGame);
                    if (compatibilityError != null)
                        return compatibilityError;

                    // Inject data
                    byte[] modifiedBlock = CreateInjectedBlock(reader, activeSlot, wonderCardBlock, selectedGame);
                    Array.Copy(modifiedBlock, 0, saveData, activeSlot + wonderCardBlock, BLOCK_SIZE);
                }

                // Write output file
                string outputPath = Path.Combine(executableDirectory, "Pokemon Injection.sav");
                File.WriteAllBytes(outputPath, saveData);

                return "Injection Successful!";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        #endregion

        #region Save Slot Management

        /// <summary>
        /// Determines which save slot is active by comparing save counters.
        /// Returns SLOT_1_OFFSET, SLOT_2_OFFSET, or -1 if no valid data exists.
        /// </summary>
        private int DetermineActiveSlot(BinaryReader reader)
        {
            bool slot1Valid = IsSlotValid(reader, SLOT_1_OFFSET);
            bool slot2Valid = IsSlotValid(reader, SLOT_2_OFFSET);

            // No valid data in either slot
            if (!slot1Valid && !slot2Valid)
                return -1;

            // Only one slot valid - use it
            if (!slot1Valid) return SLOT_2_OFFSET;
            if (!slot2Valid) return SLOT_1_OFFSET;

            // Both valid - compare save counters
            int counter1 = ReadSaveCounter(reader, SLOT_1_COUNTER_OFFSET);
            int counter2 = ReadSaveCounter(reader, SLOT_2_COUNTER_OFFSET);

            // Higher counter = more recent save
            return (counter2 > counter1) ? SLOT_2_OFFSET : SLOT_1_OFFSET;
        }

        /// <summary>
        /// Checks if a save slot contains valid data (not empty flash memory).
        /// Empty/erased flash reads as all 0xFF bytes.
        /// </summary>
        private bool IsSlotValid(BinaryReader reader, int slotOffset)
        {
            reader.BaseStream.Seek(slotOffset, SeekOrigin.Begin);
            byte[] slotData = reader.ReadBytes(SLOT_SIZE);

            // Search for any non-0xFF byte
            foreach (byte b in slotData)
            {
                if (b != EMPTY_FLASH_BYTE)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Reads 4-byte save counter and returns sum of bytes.
        /// </summary>
        private int ReadSaveCounter(BinaryReader reader, int counterOffset)
        {
            reader.BaseStream.Seek(counterOffset, SeekOrigin.Begin);
            byte[] counter = reader.ReadBytes(SAVE_COUNTER_SIZE);
            return counter[0] + counter[1] + counter[2] + counter[3];
        }

        #endregion

        #region Block Finding

        /// <summary>
        /// Searches for a block with specified marker byte within active slot.
        /// Returns block offset relative to slot start, or -1 if not found.
        /// </summary>
        private int FindBlock(BinaryReader reader, int slotOffset, byte targetMarker)
        {
            for (int blockIndex = 0; blockIndex <= MAX_BLOCK_INDEX; blockIndex++)
            {
                int markerPosition = slotOffset + (blockIndex * BLOCK_SIZE) + BLOCK_MARKER_OFFSET;
                reader.BaseStream.Seek(markerPosition, SeekOrigin.Begin);

                if (reader.ReadByte() == targetMarker)
                    return blockIndex * BLOCK_SIZE;
            }

            return -1;
        }

        /// <summary>
        /// Finds Wonder Card block (marker 0x04) containing Mystery Gift data.
        /// </summary>
        private int FindWonderCardBlock(BinaryReader reader, int slotOffset)
        {
            return FindBlock(reader, slotOffset, WONDER_CARD_BLOCK_MARKER);
        }

        /// <summary>
        /// Finds game detection block (marker 0x00) containing game type identifier.
        /// </summary>
        private int FindGameDetectionBlock(BinaryReader reader, int slotOffset)
        {
            return FindBlock(reader, slotOffset, GAME_TYPE_DETECTION_BLOCK_MARKER);
        }

        #endregion

        #region Game Type Validation

        /// <summary>
        /// Validates save file game type matches selected Mystery Gift type.
        /// Returns error message if incompatible, null if valid.
        /// </summary>
        private string ValidateGameCompatibility(BinaryReader reader, int slotOffset,
            int gameDetectionBlock, GameType selectedGame)
        {
            // Read game type marker value
            int markerPosition = slotOffset + gameDetectionBlock + GAME_TYPE_MARKER_OFFSET;
            reader.BaseStream.Seek(markerPosition, SeekOrigin.Begin);
            int gameTypeValue = BitConverter.ToInt32(reader.ReadBytes(4), 0);

            // Ruby/Sapphire incompatibility
            if (gameTypeValue == GAME_TYPE_VALUE_RUBY_SAPPHIRE)
                return "Mystery Gifts are not compatible with Ruby and Sapphire";

            // Validate type match
            GameType detectedGame = (gameTypeValue == GAME_TYPE_VALUE_FIRERED_LEAFGREEN)
                ? GameType.FireRedLeafGreen
                : GameType.Emerald;

            if (detectedGame != selectedGame)
            {
                return (selectedGame == GameType.Emerald)
                    ? "This is an Emerald Mystery Gift and cannot be placed in a Fire Red / Leaf Green save."
                    : "This is an Fire Red / Leaf Green Mystery Gift and cannot be placed in an Emerald save.";
            }

            return null;
        }

        #endregion

        #region Data Injection

        /// <summary>
        /// Creates modified Wonder Card block with injected Mystery Gift data and updated checksums.
        /// </summary>
        private byte[] CreateInjectedBlock(BinaryReader reader, int slotOffset,
            int wonderCardBlock, GameType gameType)
        {
            // Read existing block
            reader.BaseStream.Seek(slotOffset + wonderCardBlock, SeekOrigin.Begin);
            byte[] block = reader.ReadBytes(BLOCK_SIZE);

            // Get game-specific data offset
            int baseOffset = (gameType == GameType.FireRedLeafGreen)
                ? FRLG_WONDER_CARD_BASE
                : EMERALD_WONDER_CARD_BASE;

            // Inject Wonder Card data
            WriteWonderCardData(block, baseOffset);

            // Inject GMScript data
            WriteGMScriptData(block, baseOffset);

            // Update block checksum
            WriteBlockChecksum(block);

            return block;
        }

        /// <summary>
        /// Writes Wonder Card payload + CRC-16 + terminator bytes to block.
        /// </summary>
        private void WriteWonderCardData(byte[] block, int baseOffset)
        {
            // Copy Wonder Card payload (332 bytes)
            Array.Copy(wonderCardData, 0, block, baseOffset + WONDER_CARD_PAYLOAD_OFFSET, WONDER_CARD_DATA_SIZE);

            // Write CRC-16 checksum (little-endian)
            int crc = CalculateCrc16(wonderCardData);
            WriteLittleEndianUInt16(block, baseOffset + WONDER_CARD_CRC16_OFFSET, crc);

            // Write terminator marker
            block[baseOffset + WONDER_CARD_TERMINATOR_OFFSET] = 0xFF;
            block[baseOffset + WONDER_CARD_TERMINATOR_OFFSET + 1] = 0xFF;
        }

        /// <summary>
        /// Writes GMScript payload + CRC-16 to block.
        /// </summary>
        private void WriteGMScriptData(byte[] block, int baseOffset)
        {
            // Copy GMScript payload (1000 bytes)
            Array.Copy(scriptData, 0, block, baseOffset + GMSCRIPT_PAYLOAD_OFFSET, GMSCRIPT_DATA_SIZE);

            // Write CRC-16 checksum (little-endian)
            int crc = CalculateCrc16(scriptData);
            WriteLittleEndianUInt16(block, baseOffset + GMSCRIPT_CRC16_OFFSET, crc);
        }

        /// <summary>
        /// Calculates and writes block footer checksum.
        /// </summary>
        private void WriteBlockChecksum(byte[] block)
        {
            int checksum = CalculateBlockChecksum(block);
            WriteBigEndianUInt16(block, BLOCK_FOOTER_CHECKSUM_OFFSET, checksum);
        }

        #endregion

        #region Checksum Calculations

        /// <summary>
        /// Calculates CRC-16/XMODEM checksum using lookup table.
        /// Initial value: 0x1121, Final XOR: 0xFFFF (bitwise NOT)
        /// </summary>
        private int CalculateCrc16(byte[] data)
        {
            int crc = CRC16_INITIAL_VALUE;

            foreach (byte b in data)
            {
                int tableIndex = (crc ^ b) & 0xFF;
                int tableValue = crcTable[tableIndex * 2] | (crcTable[tableIndex * 2 + 1] << 8);
                crc = tableValue ^ (crc >> 8);
            }

            return ~crc & 0xFFFF;
        }

        /// <summary>
        /// Calculates block footer checksum: sum 962 little-endian 32-bit integers,
        /// add carry (lower 16 bits + upper 16 bits), then byte swap.
        /// </summary>
        private int CalculateBlockChecksum(byte[] block)
        {
            int sum = 0;

            // Sum 962 × 4-byte integers (3848 bytes total)
            for (int i = 0; i < BLOCK_CHECKSUM_INTEGER_COUNT; i++)
            {
                int offset = i * 4;
                sum += block[offset]
                     | (block[offset + 1] << 8)
                     | (block[offset + 2] << 16)
                     | (block[offset + 3] << 24);
            }

            // Add carry and byte swap
            short checksumWithCarry = (short)((sum & 0xFFFF) + (sum >> 16));
            return ((checksumWithCarry & 0xFF) << 8) | ((checksumWithCarry >> 8) & 0xFF);
        }

        #endregion

        #region Binary Write Helpers

        /// <summary>
        /// Writes 16-bit value in little-endian byte order (LSB first).
        /// </summary>
        private void WriteLittleEndianUInt16(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)(value >> 8);
        }

        /// <summary>
        /// Writes 16-bit value in big-endian byte order (MSB first).
        /// </summary>
        private void WriteBigEndianUInt16(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)(value >> 8);
            buffer[offset + 1] = (byte)(value & 0xFF);
        }

        #endregion
    }
}
