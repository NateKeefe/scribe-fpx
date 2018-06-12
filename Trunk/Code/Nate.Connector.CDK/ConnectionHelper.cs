using System;
using System.Collections.Generic;

using Scribe.Core.ConnectorApi.Exceptions;
using Scribe.Core.ConnectorApi.ConnectionUI;
using Scribe.Core.ConnectorApi.Cryptography;

namespace CDK
{
    public static class ConnectionHelper
    {
        #region Constants
        public class ConnectionProperties
        {
            public string BaseUrl { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        internal static class ConnectionPropertyKeys
        {
            public const string BaseUrl = "BaseUrl";
            public const string Username = "Username";
            public const string Password = "Password";

        }

        internal static class ConnectionPropertyLabels
        {
            public const string BaseUrl = "BaseUrl";
            public const string Username = "Username";
            public const string Password = "Password";
        }

        private const string HelpLink = "http://docs.fpx.com/docs/api/restful/";
        #endregion

        public static ConnectionProperties GetConnectionProperties(IDictionary<string, string> propDictionary)
        {
            if (propDictionary == null)
                throw new InvalidConnectionException("Connection Properties are NULL");
            //capture props
            var connectorProps = new ConnectionProperties();
            connectorProps.BaseUrl = getRequiredPropertyValue(propDictionary, ConnectionPropertyKeys.BaseUrl, ConnectionPropertyLabels.BaseUrl);
            connectorProps.Username = getRequiredPropertyValue(propDictionary, ConnectionPropertyKeys.Username, ConnectionPropertyLabels.Username);
            connectorProps.Password = getRequiredPropertyValue(propDictionary, ConnectionPropertyKeys.Password, ConnectionPropertyLabels.Password);

            //tweak data coming in
            connectorProps.Password = Decryptor.Decrypt_AesManaged(connectorProps.Password, Connector.CryptoKey);
            if (connectorProps.BaseUrl.ToString().EndsWith("/"))
                { connectorProps.BaseUrl = connectorProps.BaseUrl.Remove(connectorProps.BaseUrl.Length - 1); }

            // re-check unencrypted password
            if (string.IsNullOrEmpty(connectorProps.Password))
                throw new InvalidConnectionException(string.Format("A value is required for '{0}'", ConnectionPropertyLabels.Password));

            return connectorProps;
        }

        private static string getRequiredPropertyValue(IDictionary<string, string> properties, string key, string label)
        {
            var value = getPropertyValue(properties, key);
            if (string.IsNullOrEmpty(value))
                throw new InvalidConnectionException(string.Format("A value is required for '{0}'", label));

            return value;
        }

        private static string getPropertyValue(IDictionary<string, string> properties, string key)
        {
            var value = "";
            properties.TryGetValue(key, out value);
            return value;
        }

        public static FormDefinition GetConnectionFormDefintion()
        {

            var formDefinition = new FormDefinition
            {
                CompanyName = Connector.CompanyName,
                CryptoKey = Connector.CryptoKey,
                HelpUri = new Uri(HelpLink)
            };

            formDefinition.Add(BuildBaseUrlDefinition(0));
            formDefinition.Add(BuildUsernameDefinition(1));
            formDefinition.Add(BuildPasswordDefinition(2));

            return formDefinition;
        }

        #region Form Definition Builders
        private static EntryDefinition BuildBaseUrlDefinition(int order)
        {
            var entryDefinition = new EntryDefinition
            {
                InputType = InputType.Text,
                IsRequired = false,
                Label = ConnectionPropertyLabels.BaseUrl,
                PropertyName = ConnectionPropertyKeys.BaseUrl,
                Order = order,
            };

            return entryDefinition;
        }

        private static EntryDefinition BuildUsernameDefinition(int order)
        {
            var entryDefinition = new EntryDefinition
            {
                InputType = InputType.Text,
                IsRequired = false,
                Label = ConnectionPropertyLabels.Username,
                PropertyName = ConnectionPropertyKeys.Username,
                Order = order,
            };

            return entryDefinition;
        }

        private static EntryDefinition BuildPasswordDefinition(int order)
        {
            var entryDefinition = new EntryDefinition
            {
                InputType = InputType.Password,
                IsRequired = false,
                Label = ConnectionPropertyLabels.Password,
                PropertyName = ConnectionPropertyKeys.Password,
                Order = order,
            };

            return entryDefinition;
        }
        #endregion
    }
}