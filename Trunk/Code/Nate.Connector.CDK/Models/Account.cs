using System;
using Scribe.Connector.Common.Reflection;
using Scribe.Connector.Common.Reflection.Actions;

namespace CDK.Models.Account
{
    [ObjectDefinition(Name = "Accounts")]
    [Query]
    public class Rootobject
    {
        //Filters
        [PropertyDefinition(UsedInQueryConstraint = true, UsedInQuerySelect = false)]
        public string query { get; set; }
        [PropertyDefinition(UsedInQueryConstraint = true, UsedInQuerySelect = false)]
        public string resolvenames { get; set; }

        [PropertyDefinition]
        public int size { get; set; }
        [PropertyDefinition]
        public Record[] records { get; set; }
        [PropertyDefinition]
        public bool done { get; set; }
    }

    [ObjectDefinition(Name = "Account")]
    [Query]
    [Update]
    public class Record
    {
        [PropertyDefinition]
        public DateTime LastModifiedDate { get; set; }
        [PropertyDefinition]
        public string BillingCity { get; set; }
        [PropertyDefinition]
        public string ExternalId { get; set; }
        [PropertyDefinition]
        public string Website { get; set; }
        [PropertyDefinition]
        public string Name { get; set; }
        [PropertyDefinition]
        public string AccountNumber { get; set; }
        [PropertyDefinition]
        public string ShippingStreet { get; set; }
        [PropertyDefinition]
        public string BillingCountry { get; set; }
        [PropertyDefinition]
        public AccountCreatedbyid CreatedById { get; set; }
        [PropertyDefinition]
        public string BillingStreet { get; set; }
        [PropertyDefinition]
        public AccountOwnerid OwnerId { get; set; }
        [PropertyDefinition]
        public string Phone { get; set; }
        [PropertyDefinition]
        public string ShippingPostalCode { get; set; }
        [PropertyDefinition]
        public DateTime CreatedDate { get; set; }
        [PropertyDefinition]
        public string ShippingCountry { get; set; }
        [PropertyDefinition]
        public string BillingPostalCode { get; set; }
        [PropertyDefinition]
        public string ShippingCity { get; set; }
        [PropertyDefinition]
        public string ShippingState { get; set; }
        [PropertyDefinition(UsedInLookupCondition = true, UsedInActionInput = false)]
        public string Id { get; set; }
        [PropertyDefinition]
        public string Fax { get; set; }
        [PropertyDefinition]
        public AccountLastmodifiedbyid LastModifiedById { get; set; }
        [PropertyDefinition]
        public string BillingState { get; set; }
    }

    [ObjectDefinition]
    public class AccountCreatedbyid
    {
        [PropertyDefinition]
        public string Id { get; set; }
        [PropertyDefinition]
        public string Name { get; set; }
    }

    [ObjectDefinition]
    public class AccountOwnerid
    {
        [PropertyDefinition]
        public string Id { get; set; }
        [PropertyDefinition]
        public string Name { get; set; }
    }

    [ObjectDefinition]
    public class AccountLastmodifiedbyid
    {
        [PropertyDefinition]
        public string Id { get; set; }
        [PropertyDefinition]
        public string Name { get; set; }
    }
}