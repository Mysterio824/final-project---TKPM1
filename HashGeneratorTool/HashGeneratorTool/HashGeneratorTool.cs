using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;

namespace HashGeneratorTool
{
    class HashGeneratorTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;
        // Method to generate hash
        public string GenerateHash(string input, string algorithmType)
        {
            switch (algorithmType)
            {
                case "MD5":
                    return GenerateMD5Hash(input);
                case "SHA1":
                    return GenerateSHA1Hash(input);
                case "SHA256":
                    return GenerateSHA256Hash(input);
                case "SHA224":
                    return GenerateSHA224Hash(input);
                case "SHA512":
                    return GenerateSHA512Hash(input);
                case "SHA384":
                    return GenerateSHA384Hash(input);
                case "SHA3":
                    return GenerateSHA3Hash(input);
                case "RIPEMD160":
                    return GenerateRIPEMD160Hash(input);
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithmType), algorithmType, null);
            }
        }

        // MD5 Hash
        private string GenerateMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // SHA-1 Hash
        private string GenerateSHA1Hash(string input)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // SHA-256 Hash
        private string GenerateSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // SHA-224 Hash
        private string GenerateSHA224Hash(string input)
        {
            // SHA224 is not directly supported in .NET, using SHA256 and truncating
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                // SHA-224 is SHA-256 truncated to 224 bits (28 bytes)
                byte[] sha224Bytes = new byte[28];
                Array.Copy(hashBytes, sha224Bytes, 28);
                return BitConverter.ToString(sha224Bytes).Replace("-", "").ToLower();
            }
        }

        // SHA-512 Hash
        private string GenerateSHA512Hash(string input)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // SHA-384 Hash
        private string GenerateSHA384Hash(string input)
        {
            using (SHA384 sha384 = SHA384.Create())
            {
                byte[] hashBytes = sha384.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // SHA3 Hash (using SHA3-256 since .NET doesn't have native SHA3)
        private string GenerateSHA3Hash(string input)
        {
            return SHA3.ComputeHash(input);
        }

        // RIPEMD160 Hash
        private string GenerateRIPEMD160Hash(string input)
        {
            return RIPEMD160Hasher.ComputeHash(input);
        }
        public class SHA3
        {
            private static readonly int Rounds = 24;
            private static readonly int BlockSize = 136;  // For SHA3-256, it's 136 bytes per block

            public static string ComputeHash(string input)
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                ulong[] state = new ulong[25];

                // Pad the input to the block size
                byte[] paddedInput = PadInput(inputBytes);

                // Process input blocks
                for (int i = 0; i < paddedInput.Length / BlockSize; i++)
                {
                    byte[] block = new byte[BlockSize];
                    Array.Copy(paddedInput, i * BlockSize, block, 0, BlockSize);
                    ProcessBlock(block, state);
                }

                // Extract the hash from the state (32 bytes for SHA3-256)
                byte[] hash = new byte[32];
                Buffer.BlockCopy(state, 0, hash, 0, 32);

                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            private static byte[] PadInput(byte[] input)
            {
                int padLength = BlockSize - (input.Length % BlockSize);
                if (padLength == 0) padLength = BlockSize;

                byte[] paddedInput = new byte[input.Length + padLength];
                Array.Copy(input, paddedInput, input.Length);

                // SHA3 padding: 0x06 followed by zeros and ended with 0x80
                paddedInput[input.Length] = 0x06;
                paddedInput[paddedInput.Length - 1] |= 0x80;

                return paddedInput;
            }

            private static void ProcessBlock(byte[] block, ulong[] state)
            {
                // XOR the block into the state
                for (int i = 0; i < block.Length / 8; i++)
                {
                    state[i] ^= BitConverter.ToUInt64(block, i * 8);
                }

                // Apply the Keccak-f permutation
                KeccakF1600_StatePermute(state);
            }

            private static void KeccakF1600_StatePermute(ulong[] state)
            {
                ulong[] B = new ulong[25];
                ulong[] C = new ulong[5];
                ulong[] D = new ulong[5];

                for (int round = 0; round < Rounds; round++)
                {
                    // Theta step
                    for (int x = 0; x < 5; x++)
                    {
                        C[x] = state[x] ^ state[x + 5] ^ state[x + 10] ^ state[x + 15] ^ state[x + 20];
                    }

                    for (int x = 0; x < 5; x++)
                    {
                        D[x] = C[(x + 4) % 5] ^ ROL64(C[(x + 1) % 5], 1);

                        for (int y = 0; y < 5; y++)
                        {
                            state[x + 5 * y] ^= D[x];
                        }
                    }

                    // Rho and Pi steps
                    ulong t = state[1];
                    for (int x = 0, y = 0, current = 0; current < 24; current++)
                    {
                        int newX = y;
                        int newY = (2 * x + 3 * y) % 5;

                        int index = newX + 5 * newY;
                        ulong newT = state[index];

                        state[index] = ROL64(t, (current + 1) * (current + 2) / 2 % 64);

                        x = newX;
                        y = newY;
                        t = newT;
                    }

                    // Chi step
                    for (int y = 0; y < 5; y++)
                    {
                        for (int x = 0; x < 5; x++)
                        {
                            B[x + 5 * y] = state[x + 5 * y];
                        }
                    }

                    for (int y = 0; y < 5; y++)
                    {
                        for (int x = 0; x < 5; x++)
                        {
                            state[x + 5 * y] = B[x + 5 * y] ^ ((~B[(x + 1) % 5 + 5 * y]) & B[(x + 2) % 5 + 5 * y]);
                        }
                    }

                    // Iota step
                    state[0] ^= KeccakRoundConstants[round];
                }
            }

            private static ulong ROL64(ulong x, int n)
            {
                return (x << n) | (x >> (64 - n));
            }

            private static readonly ulong[] KeccakRoundConstants = new ulong[]
            {
                0x0000000000000001UL, 0x0000000000008082UL, 0x800000000000808aUL, 0x8000000080008000UL,
                0x000000000000808bUL, 0x0000000080000001UL, 0x8000000080008081UL, 0x8000000000008009UL,
                0x000000000000008aUL, 0x0000000000000088UL, 0x0000000080008009UL, 0x000000008000000aUL,
                0x000000008000808bUL, 0x800000000000008bUL, 0x8000000000008089UL, 0x8000000000008003UL,
                0x8000000000008002UL, 0x8000000000000080UL, 0x000000000000800aUL, 0x800000008000000aUL,
                0x8000000080008081UL, 0x8000000000008080UL, 0x0000000080000001UL, 0x8000000080008008UL
            };
        }
        public class RIPEMD160Hasher
        {
            private static readonly uint[] K1 = { 0x00000000, 0x5A827999, 0x6ED9EBA1, 0x8F1BBCDC, 0xA953FD4E };
            private static readonly uint[] K2 = { 0x50A28BE6, 0x5C4DD124, 0x6D703EF3, 0x7A6D76E9, 0x00000000 };

            private static readonly int[] R1 = {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
                7, 4, 13, 1, 10, 6, 15, 3, 12, 0, 9, 5, 2, 14, 11, 8,
                3, 10, 14, 4, 9, 15, 8, 1, 2, 7, 0, 6, 13, 11, 5, 12,
                1, 9, 11, 10, 0, 8, 12, 4, 13, 3, 7, 15, 14, 5, 6, 2,
                4, 0, 5, 9, 7, 12, 2, 10, 14, 1, 3, 8, 11, 6, 15, 13
            };

            private static readonly int[] R2 = {
                5, 14, 7, 0, 9, 2, 11, 4, 13, 6, 15, 8, 1, 10, 3, 12,
                6, 11, 3, 7, 0, 13, 5, 10, 14, 15, 8, 12, 4, 9, 1, 2,
                15, 5, 1, 3, 7, 14, 6, 9, 11, 8, 12, 2, 10, 0, 13, 4,
                8, 6, 4, 1, 3, 11, 15, 0, 5, 12, 2, 13, 9, 7, 10, 14,
                12, 15, 10, 4, 1, 5, 8, 7, 6, 2, 13, 14, 0, 3, 9, 11
            };

            private static readonly int[] S1 = {
                11, 14, 15, 12, 5, 8, 7, 9, 11, 13, 14, 15, 6, 7, 9, 8,
                7, 6, 8, 13, 11, 9, 7, 15, 7, 12, 15, 9, 11, 7, 13, 12,
                11, 13, 6, 7, 14, 9, 13, 15, 14, 8, 13, 6, 5, 12, 7, 5,
                11, 12, 14, 15, 14, 15, 9, 8, 9, 14, 5, 6, 8, 6, 5, 12,
                9, 15, 5, 11, 6, 8, 13, 12, 5, 12, 13, 14, 11, 8, 5, 6
            };

            private static readonly int[] S2 = {
                8, 9, 9, 11, 13, 15, 15, 5, 7, 7, 8, 11, 14, 14, 12, 6,
                9, 13, 15, 7, 12, 8, 9, 11, 7, 7, 12, 7, 6, 15, 13, 11,
                9, 7, 15, 11, 8, 6, 6, 14, 12, 13, 5, 14, 13, 13, 7, 5,
                15, 5, 8, 11, 14, 14, 6, 14, 6, 9, 12, 9, 12, 5, 15, 8,
                8, 5, 12, 9, 12, 5, 14, 6, 8, 13, 6, 5, 15, 13, 11, 11
            };

            private static uint F(int j, uint x, uint y, uint z)
            {
                if (j <= 15) return x ^ y ^ z;
                if (j <= 31) return (x & y) | (~x & z);
                if (j <= 47) return (x | ~y) ^ z;
                if (j <= 63) return (x & z) | (y & ~z);
                return x ^ (y | ~z);
            }

            private static uint RL(uint x, int n) => (x << n) | (x >> (32 - n));

            public static string ComputeHash(string input)
            {
                byte[] data = Encoding.UTF8.GetBytes(input);
                int paddedLength = ((data.Length + 8) / 64 + 1) * 64;
                byte[] padded = new byte[paddedLength];
                Array.Copy(data, padded, data.Length);
                padded[data.Length] = 0x80;

                ulong bitLength = (ulong)data.Length * 8;
                byte[] bitLengthBytes = BitConverter.GetBytes(bitLength);
                Array.Copy(bitLengthBytes, 0, padded, padded.Length - 8, 8);

                uint h0 = 0x67452301;
                uint h1 = 0xEFCDAB89;
                uint h2 = 0x98BADCFE;
                uint h3 = 0x10325476;
                uint h4 = 0xC3D2E1F0;

                for (int i = 0; i < padded.Length; i += 64)
                {
                    uint[] X = new uint[16];
                    for (int j = 0; j < 16; j++)
                        X[j] = BitConverter.ToUInt32(padded, i + j * 4);

                    uint A1 = h0, B1 = h1, C1 = h2, D1 = h3, E1 = h4;
                    uint A2 = h0, B2 = h1, C2 = h2, D2 = h3, E2 = h4;

                    for (int j = 0; j < 80; j++)
                    {
                        uint T = RL(A1 + F(j, B1, C1, D1) + X[R1[j]] + K1[j / 16], S1[j]) + E1;
                        A1 = E1; E1 = D1; D1 = RL(C1, 10); C1 = B1; B1 = T;

                        T = RL(A2 + F(79 - j, B2, C2, D2) + X[R2[j]] + K2[j / 16], S2[j]) + E2;
                        A2 = E2; E2 = D2; D2 = RL(C2, 10); C2 = B2; B2 = T;
                    }

                    uint T0 = h1 + C1 + D2;
                    uint T1 = h2 + D1 + E2;
                    uint T2 = h3 + E1 + A2;
                    uint T3 = h4 + A1 + B2;
                    uint T4 = h0 + B1 + C2;

                    h0 = T0;
                    h1 = T1;
                    h2 = T2;
                    h3 = T3;
                    h4 = T4;
                }

                byte[] hash = BitConverter.GetBytes(h0)
                    .Concat(BitConverter.GetBytes(h1))
                    .Concat(BitConverter.GetBytes(h2))
                    .Concat(BitConverter.GetBytes(h3))
                    .Concat(BitConverter.GetBytes(h4))
                    .ToArray();

                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public object Execute(object input) { return input; }

        public UserControl GetUI()
        {
            return new HashGeneratorToolUI(this);
        }
    }
}