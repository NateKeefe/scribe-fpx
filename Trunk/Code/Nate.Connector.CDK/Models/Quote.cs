using Newtonsoft.Json;
using Scribe.Connector.Common.Reflection;
using Scribe.Connector.Common.Reflection.Actions;
using System;

namespace CDK.Models.Quote
{
    [ObjectDefinition(Name = "Quotes")]
    [Query]
    public class Rootobject
    {
        [PropertyDefinition]
        public int size { get; set; }
        [PropertyDefinition]
        public Record[] records { get; set; }
        [PropertyDefinition]
        public bool done { get; set; }
    }

    [ObjectDefinition(Name = "Quote")]
    [Query]
    [Update]
    public class Record
    {
        [PropertyDefinition]
        public DateTime OrderStatusUpdated__c { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public DateTime LastModifiedDate { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public string Description { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public string FormattedIdBase { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public string Name { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public int TotalCost { get; set; }
        [PropertyDefinition]
        public string OrderId__c { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public Createdbyid CreatedById { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public QuoteOwnerid OwnerId { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public QuoteOpportunityid OpportunityId { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public int TotalProductsCost { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public bool IsProductUpdateAllowed { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public int ProductsAmount { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public bool IsDependentDataOutOfSync { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public int ProductsCount { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public int TotalProfit { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public string LastExportedDate { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public string ClonedFromId { get; set; }
        [PropertyDefinition]
        public string OrderStatus__c { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public int TotalAmount { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public string FormattedId { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public string CurrencyIsoCode { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public string FormattedIdSuffix { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public DateTime ExpirationDate { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public string Note { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public DateTime CreatedDate { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public bool IsPrimary { get; set; }
        [PropertyDefinition(UsedInQueryConstraint = true, UsedInQuerySelect = true, UsedInLookupCondition = true, UsedInActionInput = false)]
        [JsonIgnore]
        public string Id { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public float TotalMargin { get; set; }
        [PropertyDefinition]
        [JsonIgnore]
        public QuoteLastmodifiedbyid LastModifiedById { get; set; }
    }

    [ObjectDefinition]
    public class Createdbyid
    {
        [PropertyDefinition]
        public string Id { get; set; }
        [PropertyDefinition]
        public string Name { get; set; }
    }

    [ObjectDefinition]
    public class QuoteOwnerid
    {
        [PropertyDefinition]
        public string Id { get; set; }
        [PropertyDefinition]
        public string Name { get; set; }
    }

    [ObjectDefinition]
    public class QuoteOpportunityid
    {
        [PropertyDefinition]
        public string Id { get; set; }
        [PropertyDefinition]
        public string Name { get; set; }
    }

    [ObjectDefinition]
    public class QuoteLastmodifiedbyid
    {
        [PropertyDefinition]
        public string Id { get; set; }
        [PropertyDefinition]
        public string Name { get; set; }
    }
}
