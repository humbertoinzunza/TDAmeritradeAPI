﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TDAmeritradeAPI
{
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            string newName = "";

            for (int i = 1; i < name.Length; i++)
            {
                if (char.IsUpper(name[i - 1]))
                    newName += '_';
                newName += name[i];
            }

            return newName;
        }
    }

    public class InstrumentConverter : JsonConverter<Instrument>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Instrument).IsAssignableFrom(typeToConvert);
        }

        public override Instrument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Utf8JsonReader readerClone = reader;


            if (readerClone.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? propertyName = readerClone.GetString();
            while (propertyName != "assetType")
            {
                readerClone.Read();
                propertyName = readerClone.GetString();
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            Instrument.AssetTypes typeDiscriminator = Enum.Parse<Instrument.AssetTypes>(readerClone.GetString()!);
            Instrument instrument = typeDiscriminator switch
            {
                Instrument.AssetTypes.EQUITY => new Equity(),
                Instrument.AssetTypes.OPTION => new Option(),
                Instrument.AssetTypes.FIXED_INCOME => new FixedIncome(),
                Instrument.AssetTypes.MUTUAL_FUND => new MutualFund(),
                Instrument.AssetTypes.CASH_EQUIVALENT => new CashEquivalent(),
                _ => new Instrument()
            };

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return instrument;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "assetType":
                            instrument.AssetType = Enum.Parse<Instrument.AssetTypes>(reader.GetString()!);
                            break;
                        case "cusip":
                            instrument.Cusip = reader.GetString()!;
                            break;
                        case "symbol":
                            instrument.Symbol = reader.GetString()!;
                            break;
                        case "description":
                            instrument.Description = reader.GetString()!;
                            break;
                        case "maturityDate":
                            ((FixedIncome)instrument).MaturityDate = reader.GetString()!;
                            break;
                        case "variableRate":
                            ((FixedIncome)instrument).VariableRate = reader.GetDouble();
                            break;
                        case "factor":
                            ((FixedIncome)instrument).Factor = reader.GetDouble();
                            break;
                        case "type":
                            switch (typeDiscriminator)
                            {
                                case Instrument.AssetTypes.OPTION:
                                    ((Option)instrument).Type = Enum.Parse<Option.Enums.Type>(reader.GetString()!);
                                    break;
                                case Instrument.AssetTypes.MUTUAL_FUND:
                                    ((MutualFund)instrument).Type = Enum.Parse<MutualFund.Enums.Type>(reader.GetString()!);
                                    break;
                                case Instrument.AssetTypes.CASH_EQUIVALENT:
                                    ((CashEquivalent)instrument).Type = Enum.Parse<CashEquivalent.Enums.Type>(reader.GetString()!);
                                    break;
                                default:
                                    throw new JsonException();
                            }
                            break;
                        case "putCall":
                            ((Option)instrument).PutCall = Enum.Parse<Option.Enums.PutCall>(reader.GetString()!);
                            break;
                        case "underlyingSymbol":
                            ((Option)instrument).UnderlyingSymbol = reader.GetString()!;
                            break;
                        case "optionMultiplier":
                            ((Option)instrument).OptionMultiplier = reader.GetInt32();
                            break;
                        case "optionDeliverables":
                            ((Option)instrument).OptionDeliverables = new List<Option.Structs.OptionDeliverable>();

                            Option.Structs.OptionDeliverable optionDeliverable = new();
                            while (reader.TokenType != JsonTokenType.EndArray)
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.PropertyName)
                                {
                                    propertyName = reader.GetString();
                                    reader.Read();
                                    switch (propertyName)
                                    {
                                        case "symbol":
                                            optionDeliverable.Symbol = reader.GetString(); break;
                                        case "deliverableUnits":
                                            optionDeliverable.DeliverableUnits = reader.GetDouble(); break;
                                        case "currencyType":
                                            optionDeliverable.CurrencyType = Enum.Parse<Option.Enums.CurrencyType>(reader.GetString()!); break;
                                        case "assetType":
                                            optionDeliverable.AssetType = Enum.Parse<Instrument.AssetTypes>(reader.GetString()!); break;
                                    }
                                }
                                if (reader.TokenType == JsonTokenType.EndObject)
                                {
                                    ((Option)instrument).OptionDeliverables!.Add(optionDeliverable);
                                    optionDeliverable = new();
                                }
                            }
                            break;
                    }
                }
            }

            return instrument;
        }
        public override void Write(
            Utf8JsonWriter writer, Instrument instrument, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (instrument.AssetType is not null)
                writer.WriteString("assetType", instrument.AssetType.ToString());
            if (instrument.Symbol is not null)
                writer.WriteString("symbol", instrument.Symbol);
            if (instrument.Description is not null)
                writer.WriteString("description", instrument.Description);

            if (instrument is FixedIncome fixedIncome)
            {
                if (fixedIncome.MaturityDate is not null)
                    writer.WriteString("maturityDate", fixedIncome.MaturityDate);
                if (fixedIncome.VariableRate is not null)
                    writer.WriteNumber("variableRate", (double)fixedIncome.VariableRate);
                if(fixedIncome.Factor is not null)
                    writer.WriteNumber("factor", (double)fixedIncome.Factor);
            }
            else if (instrument is MutualFund mutualFund)
            {
                if (mutualFund.Type is not null)
                    writer.WriteString("type", mutualFund.Type.ToString());
            }
            else if (instrument is CashEquivalent cashEquivalent)
            {
                if (cashEquivalent.Type is not null)
                    writer.WriteString("type", cashEquivalent.Type.ToString());
            }
            else if (instrument is Option option)
            {
                if (option.Type is not null)
                    writer.WriteString("type", option.Type.ToString());
                if (option.PutCall is not null)
                    writer.WriteString("putCall", option.PutCall.ToString());
                if (option.UnderlyingSymbol is not null)
                    writer.WriteString("underlyingSymbol", option.UnderlyingSymbol);
                if (option.OptionMultiplier is not null)
                    writer.WriteNumber("optionMultiplier", (int)option.OptionMultiplier);
                if (option.OptionDeliverables is not null && option.OptionDeliverables.Count > 0)
                {
                    writer.WriteStartArray("optionDeliverables");
                    foreach (Option.Structs.OptionDeliverable oDeliverable in option.OptionDeliverables)
                    {
                        if (oDeliverable.Symbol is not null)
                            writer.WriteString("symbol", oDeliverable.Symbol);
                        if (oDeliverable.DeliverableUnits is not null)
                            writer.WriteNumber("deliverableUnits", (double)oDeliverable.DeliverableUnits);
                        if (oDeliverable.CurrencyType is not null)
                            writer.WriteString("currencyType", oDeliverable.CurrencyType.ToString());
                        if (oDeliverable.AssetType is not null)
                            writer.WriteString("assetType", oDeliverable.AssetType.ToString());
                    }
                    writer.WriteEndArray();
                }
            }
            writer.WriteEndObject();
        }
    }
}

