using Microsoft.AspNetCore.HttpOverrides;
using NBitcoin;
using NBitcoin.RPC;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrecoBot.ClientBot.Methods
{
    public static class CryptoWalletValidator
    {
        public static bool IsValidCryptoWallet(string wallet, string changePair)
        {
            switch (changePair.ToLower())
            {
                case "tether/usd":
                    return IsValidEthereumAddress(wallet);

                case "bitcoin/usd":
                    return IsValidBitcoinAddress(wallet);

                case "ethereum/usd":
                    return IsValidEthereumAddress(wallet);

                case "litecoin/usd":
                    return Regex.IsMatch(changePair, @"^^[LM3][a-km-zA-HJ-NP-Z1-9]{26,33}$"); //пока без него
                    
                /*case "cardano/usd":
                    return Regex.IsMatch(changePair, @"^[Aa][1-9a-km-zA-HJ-NP-Z]{58}$");*/

                case "dai/usd":
                    return IsValidEthereumAddress(wallet);

                case "tron/usd":
                    return IsValidTronAddress(wallet);

                case "bitcoin-cash/usd":
                    return IsValidBitcoinCashAddress(wallet); //пока без него

                case "binance-usd/usd":
                    return IsValidEthereumAddress(wallet);

                case "tontoken/usd":
                    return Regex.IsMatch(changePair, @"^0x[a-fA-F0-9]{40}$"); // пока без него

                case "dash/usd":
                    return Regex.IsMatch(changePair, @"^X[a-km-zA-HJ-NP-Z1-9]{33}$"); // пока без него

                case "verse-bitcoin/usd":
                    return IsValidEthereumAddress(wallet);

                case "dogecoin/usd":
                    return Regex.IsMatch(changePair, @"^D{1}[5-9A-HJ-NP-U]{1}[1-9A-HJ-NP-Za-km-z]{32}$"); //пока без него

                case "matic-network/usd":
                    return IsValidEthereumAddress(wallet);

                case "binancecoin/usd":
                    return IsValidEthereumAddress(wallet);

                case "usd-coin/usd":
                    return IsValidEthereumAddress(wallet);

                case "monero/usd":
                    return Regex.IsMatch(changePair, @"^4[0-9AB][123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz]{93}$"); //пока без него

                default:
                    return false;
            }
        }
        public static bool IsValidBitcoinAddress(string bitcoinAddress)
        {
            try
            {
                BitcoinAddress address = BitcoinAddress.Create(bitcoinAddress, Network.Main);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidEthereumAddress(string ethereumAddress)
        {
            try
            {
                var addressUtil = new AddressUtil();
                var isValid = addressUtil.IsValidEthereumAddressHexFormat(ethereumAddress);
                return isValid;
            }
            catch
            {
                return false;
            }
        }
        public static bool IsValidTronAddress(string address)
        {
            if (address.StartsWith("T") && address.Length == 34)
            {
                // Проверьте, что адрес начинается с "T" и имеет длину 34 символа.
                return true;
            }
            return false;
        }
        public static bool IsValidBitcoinCashAddress(string address)
        {
            try
            {
                // Убираем пробелы и ненужные символы
                address = address.Replace(" ", "").Replace("-", "");

                // Проверяем длину адреса
                if (address.Length < 26 || address.Length > 35)
                {
                    return false;
                }

                // Проверяем, что адрес начинается с "q" или "p" (для основной сети) или "bchtest" (для тестовой сети)
                if (!(address.StartsWith("q", StringComparison.OrdinalIgnoreCase) || address.StartsWith("p", StringComparison.OrdinalIgnoreCase) || address.StartsWith("bchtest", StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                // Проверяем, что адрес содержит только допустимые символы
                foreach (char c in address)
                {
                    if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
