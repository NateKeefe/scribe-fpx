using System;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;
using HttpUtils;

using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Metadata;
using Scribe.Core.ConnectorApi.Query;
using Scribe.Core.ConnectorApi.Exceptions;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Connector.Common.Reflection.Data;

using CDK.Objects;
using CDK.Common;
using CDK.Models.Account;
using CDK.Models.Quote;

namespace CDK
{
    class ConnectorService
    {
        #region Instaniation
        public RestClient client = new RestClient();
        private Reflector reflector;
        private ConnectionHelper.ConnectionProperties properties;
        public bool IsConnected { get; set; }
        public Guid ConnectorTypeId { get; }
        private DateTime? LastConnected { get; set; }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Connection
        public enum SupportedActions
        {
            Query,
            Update
        }

        public void Connect()
        {
            if (!string.IsNullOrEmpty(properties.BaseUrl) || !string.IsNullOrEmpty(properties.Username) || !string.IsNullOrEmpty(properties.Password))
            {
                try
                {
                    var url = properties.BaseUrl + "/login?expiration=10";
                    var content = "{ ";
                    content += $"\"username\":\"{properties.Username}\"";
                    content += $", \"password\":\"{properties.Password}\"";
                    content += " }";

                    client.EndPoint = url;
                    client.Method = HttpVerb.POST;
                    client.ContentType = "application/json";
                    client.Accept = "application/json";
                    client.PostData = content;

                    var result = client.MakeRequest("");

                    //if no exception, connect was successful
                    LastConnected = DateTime.UtcNow;
                    IsConnected = true;
                    reflector = new Reflector(Assembly.GetExecutingAssembly());
                }
                catch (RESTRequestException ex)
                {
                    IsConnected = false;
                    throw new InvalidConnectionException(ex.Message);
                }
            }
            IsConnected = true;
        }

        public void Connect(ConnectionHelper.ConnectionProperties properties)
        {
            this.properties = properties;
            Connect();
        }

        public void Disconnect()
        {
            IsConnected = false;
        }

        private void EnsureConnected()
        {
            if (!LastConnected.HasValue)
            {
                Connect(properties);
            }
            else if ((DateTime.UtcNow - LastConnected) > TimeSpan.FromMinutes(9.0))
            {
                Disconnect();
                Connect();
            }
        }
        #endregion

        #region Operations
        public OperationResult Update(DataEntity dataEntity, Dictionary<string, object> matchCriteria)
        {
            EnsureConnected();
            var entityName = dataEntity.ObjectDefinitionFullName;
            var operationResult = new OperationResult();

            switch (entityName)
            {
                case EntityNames.Account:
                    var account = ToScribeModel<Models.Account.Record>(dataEntity);
                    var accountRequest = HttpCall(entityName, account, matchCriteria, "update");
                    operationResult.Success = new[] { true };
                    operationResult.ObjectsAffected = new[] { 1 };
                    break;
                case EntityNames.Quote:
                    var quote = ToScribeModel<Models.Quote.Record>(dataEntity);
                    var quoteRequest = HttpCall(entityName, quote, matchCriteria, "update");
                    operationResult.Success = new[] { true };
                    operationResult.ObjectsAffected = new[] { 1 };
                    break;
                default:
                    throw new ArgumentException($"{entityName} is not supported for Create.");
            }
            return operationResult;
        }

        private T ToScribeModel<T>(DataEntity input) where T : new()
        {
            T scribeModel;
            try
            {
                scribeModel = reflector.To<T>(input);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Error translating from DataEntity to ScribeModel: " + e.Message, e);
            }
            return scribeModel;
        }
        
        private string HttpCall<T>(string entityName, T data, Dictionary<string, object> matchCriteria, string action)
        {
            client.Accept = "application/json";
            client.ContentType = "application/json";
            client.Method = HttpVerb.PUT;            //check action to set verb here instead
            var dtSettings = new JsonSerializerSettings { DateFormatString = "yyyy-MM-ddTH:mm:ss.fffZ" };

            switch (entityName)
            {
                case EntityNames.Quote:
                    try
                    {
                        matchCriteria.TryGetValue("Id", out var quoteId);
                        client.EndPoint = properties.BaseUrl + "/" + quoteId.ToString() + "?auto-product-resave=true&changes=true";
                        client.PostData = JsonConvert.SerializeObject(data, Formatting.Indented, dtSettings);
                        var response = client.MakeRequest("");
                    }
                    catch (RESTRequestException ex)
                    {
                        Logger.Write(Logger.Severity.Error,
                            $"Error on update for entity: {entityName}.", ex.InnerException.Message);
                        throw new InvalidExecuteOperationException($"Error on update for {entityName}: " + ex.Message);
                    }
                    break;
            }

            return null;
        }
        
        #endregion

        #region Query
        public IEnumerable<DataEntity> ExecuteQuery(Query query)
        {
            EnsureConnected();
            var entityName = query.RootEntity.ObjectDefinitionFullName;
            var constraints = BuildConstraintDictionary(query.Constraints);

            switch (entityName)
            {
                case EntityNames.Accounts: //All Accounts
                    return QueryApi<Models.Account.Rootobject>(query, reflector, constraints, entityName, client);
                case EntityNames.Quotes: //All Quotes
                    return QueryApi<Models.Quote.Rootobject>(query, reflector, constraints, entityName, client);
                case EntityNames.Quote: //Quotes by ID
                    return QueryApi<Models.Quote.Record>(query, reflector, constraints, entityName, client);
                case EntityNames.Account: //Accounts by ID
                    return QueryApi<Models.Account.Record>(query, reflector, constraints, entityName, client);
                default:
                    throw new InvalidExecuteQueryException(
                        $"The {entityName} entity is not supported for query.");
            }
        }

        public static IEnumerable<DataEntity> QueryApi<T>(Query query, Reflector r, Dictionary<string, object> filters, string entityName, RestClient client)
        {
            client.Method = HttpVerb.GET;
            client.Accept = "application/json";
            client.ContentType = "application/json";
            client.PostData = "";

            switch (entityName)
            {
                case EntityNames.Accounts:
                    filters.TryGetValue("query", out var queryFilter);
                    filters.TryGetValue("resolvenames", out var resolvenamesFilter);
                    try
                    {
                        client.EndPoint = "https://dm04.fpx.com/rs/19/cpq?resolve-names=true&query=select Id, CreatedById, CreatedDate, LastModifiedDate, LastModifiedById, Name, AccountNumber, BillingStreet, BillingCity, BillingState, BillingPostalCode, BillingCountry, ShippingStreet, ShippingCity, ShippingState, ShippingPostalCode, ShippingCountry, OwnerId, Phone, Fax, Website, ExternalId  from account";
                        var response = client.MakeRequest("");
                        var data = JsonConvert.DeserializeObject<Models.Account.Rootobject>(response);
                        return r.ToDataEntities(new[] { data }, query.RootEntity);
                    }
                    catch (RESTRequestException ex) 
                    {
                        Logger.Write(Logger.Severity.Error,
                            $"Error on query for entity: {entityName}.", ex.InnerException.Message);
                        throw new InvalidExecuteQueryException($"Error on query for {entityName}: " + ex.Message);
                    }
                case EntityNames.Quotes:
                    try
                    {
                        client.EndPoint = "https://dm04.fpx.com/rs/19/cpq?resolve-names=true&query=select Id, CreatedById, CreatedDate, LastModifiedDate, LastModifiedById, FormattedIdBase, FormattedIdSuffix, FormattedId, OpportunityId, Name, Description, OwnerId, ExpirationDate, TotalAmount, ProductsAmount, TotalCost, TotalProfit, TotalMargin, TotalProductsCost, LastExportedDate, Note, IsPrimary, ClonedFromId, CurrencyIsoCode, IsDependentDataOutOfSync, IsProductUpdateAllowed, ProductsCount, OrderId__c, OrderStatus__c, OrderStatusUpdated__c from quote";
                        var response = client.MakeRequest("");
                        var data = JsonConvert.DeserializeObject<Models.Quote.Rootobject>(response);
                        return r.ToDataEntities(new[] { data }, query.RootEntity);
                    }
                    catch (RESTRequestException ex)
                    {
                        Logger.Write(Logger.Severity.Error, 
                            $"Error on query for entity: {entityName}.", ex.InnerException.Message);
                        throw new InvalidExecuteQueryException($"Error on query for {entityName}: " + ex.Message);
                    }
                case EntityNames.Quote:
                    try
                    {
                        filters.TryGetValue("Id", out var Id);
                        client.EndPoint = "https://dm04.fpx.com/rs/19/cpq/" + Id.ToString();
                        var response = client.MakeRequest("");
                        var data = JsonConvert.DeserializeObject<Models.Quote.Record>(response);
                        return r.ToDataEntities(new[] { data }, query.RootEntity);
                    }
                    catch (RESTRequestException ex)
                    {
                        Logger.Write(Logger.Severity.Error,
                            $"Error on query for entity: {entityName}.", ex.InnerException.Message);
                        throw new InvalidExecuteQueryException($"Error on query for {entityName}: " + ex.Message);
                    }
                case EntityNames.Account:
                    try
                    {
                        filters.TryGetValue("Id", out var Id);
                        client.EndPoint = "https://dm04.fpx.com/rs/19/cpq/" + Id.ToString();
                        var response = client.MakeRequest("");
                        var data = JsonConvert.DeserializeObject<Models.Account.Record>(response);
                        return r.ToDataEntities(new[] { data }, query.RootEntity);
                    }
                    catch (RESTRequestException ex)
                    {
                        Logger.Write(Logger.Severity.Error,
                            $"Error on query for entity: {entityName}.", ex.InnerException.Message);
                        throw new InvalidExecuteQueryException($"Error on query for {entityName}: " + ex.Message);
                    }
                default:
                    throw new InvalidExecuteQueryException($"The {entityName} entity is not supported for query.");
            }
        }

        private static Dictionary<string, object> BuildConstraintDictionary(Expression queryExpression)
        {
            var constraints = new Dictionary<string, object>();

            if (queryExpression == null)
                return constraints;

            if (queryExpression.ExpressionType == ExpressionType.Comparison)
            {
                // only 1 filter
                addCompEprToConstraints(queryExpression as ComparisonExpression, ref constraints);
            }
            else if (queryExpression.ExpressionType == ExpressionType.Logical)
            {
                // Multiple filters
                addLogicalEprToConstraints(queryExpression as LogicalExpression, ref constraints);
            }
            else
                throw new InvalidExecuteQueryException("Unsupported filter type: " + queryExpression.ExpressionType.ToString());

            return constraints;
        }

        private static void addLogicalEprToConstraints(LogicalExpression exp, ref Dictionary<string, object> constraints)
        {
            if (exp.Operator != LogicalOperator.And)
                throw new InvalidExecuteQueryException("Unsupported operator in filter: " + exp.Operator.ToString());

            if (exp.LeftExpression.ExpressionType == ExpressionType.Comparison)
                addCompEprToConstraints(exp.LeftExpression as ComparisonExpression, ref constraints);
            else if (exp.LeftExpression.ExpressionType == ExpressionType.Logical)
                addLogicalEprToConstraints(exp.LeftExpression as LogicalExpression, ref constraints);
            else
                throw new InvalidExecuteQueryException("Unsupported filter type: " + exp.LeftExpression.ExpressionType.ToString());

            if (exp.RightExpression.ExpressionType == ExpressionType.Comparison)
                addCompEprToConstraints(exp.RightExpression as ComparisonExpression, ref constraints);
            else if (exp.RightExpression.ExpressionType == ExpressionType.Logical)
                addLogicalEprToConstraints(exp.RightExpression as LogicalExpression, ref constraints);
            else
                throw new InvalidExecuteQueryException("Unsupported filter type: " + exp.RightExpression.ExpressionType.ToString());
        }

        private static void addCompEprToConstraints(ComparisonExpression exp, ref Dictionary<string, object> constraints)
        {
            if (exp.Operator != ComparisonOperator.Equal)
                throw new InvalidExecuteQueryException(string.Format(StringMessages.OnlyEqualsOperatorAllowed, exp.Operator.ToString(), exp.LeftValue.Value));

            var constraintKey = exp.LeftValue.Value.ToString();
            if (constraintKey.LastIndexOf(".") > -1)
            {
                // need to remove "objectname." if present
                constraintKey = constraintKey.Substring(constraintKey.LastIndexOf(".") + 1);
            }
            constraints.Add(constraintKey, exp.RightValue.Value.ToString());
        }

        #endregion

        #region Metadata
        public IMetadataProvider GetMetadataProvider()
        {
            return reflector.GetMetadataProvider();
        }

        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(bool shouldGetProperties = false, bool shouldGetRelations = false)
        {
            throw new NotImplementedException();
        }

        public IObjectDefinition RetrieveObjectDefinition(string objectName, bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        public IMethodDefinition RetrieveMethodDefinition(string objectName, bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        public void ResetMetadata()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}