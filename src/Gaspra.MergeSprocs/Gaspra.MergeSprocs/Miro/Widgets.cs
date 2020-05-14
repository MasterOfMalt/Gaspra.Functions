using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gaspra.MergeSprocs.Miro
{
    public partial class Widgets
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public Datum[] Data { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }

    public partial class Datum
    {
        [JsonProperty("type")]
        public DatumType Type { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("rotation")]
        public long Rotation { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
        /*
        [JsonProperty("style")]
        public Style Style { get; set; }

        [JsonProperty("modifiedAt")]
        public DateTimeOffset ModifiedAt { get; set; }

        [JsonProperty("modifiedBy")]
        public EdBy ModifiedBy { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("createdBy")]
        public EdBy CreatedBy { get; set; }

        [JsonProperty("capabilities")]
        public Capabilities Capabilities { get; set; }
        */
    }

    public partial class Capabilities
    {
        [JsonProperty("editable")]
        public bool Editable { get; set; }
    }

    public partial class EdBy
    {
        [JsonProperty("type")]
        public CreatedByType Type { get; set; }

        [JsonProperty("name")]
        public Name Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class Style
    {
        [JsonProperty("backgroundColor")]
        public BackgroundColor BackgroundColor { get; set; }

        [JsonProperty("backgroundOpacity")]
        public double BackgroundOpacity { get; set; }

        [JsonProperty("borderColor")]
        public Color BorderColor { get; set; }

        [JsonProperty("borderOpacity")]
        public long BorderOpacity { get; set; }

        [JsonProperty("borderStyle")]
        public BorderStyle BorderStyle { get; set; }

        [JsonProperty("borderWidth")]
        public long BorderWidth { get; set; }

        [JsonProperty("fontFamily")]
        public FontFamily FontFamily { get; set; }

        [JsonProperty("fontSize")]
        public long FontSize { get; set; }

        [JsonProperty("shapeType")]
        public ShapeType ShapeType { get; set; }

        [JsonProperty("textAlign")]
        public TextAlign TextAlign { get; set; }

        [JsonProperty("textAlignVertical")]
        public TextAlignVertical TextAlignVertical { get; set; }

        [JsonProperty("textColor")]
        public Color TextColor { get; set; }
    }

    public enum Name { RichardLawrence };

    public enum CreatedByType { User };

    public enum BackgroundColor { The00A6Ff };

    public enum Color { Ff7B00 };

    public enum BorderStyle { Normal };

    public enum FontFamily { OpenSans };

    public enum ShapeType { Rectangle };

    public enum TextAlign { Center };

    public enum TextAlignVertical { Middle };

    public enum DatumType { Shape };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                NameConverter.Singleton,
                CreatedByTypeConverter.Singleton,
                BackgroundColorConverter.Singleton,
                ColorConverter.Singleton,
                BorderStyleConverter.Singleton,
                FontFamilyConverter.Singleton,
                ShapeTypeConverter.Singleton,
                TextAlignConverter.Singleton,
                TextAlignVerticalConverter.Singleton,
                DatumTypeConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class NameConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Name) || t == typeof(Name?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "Richard Lawrence")
            {
                return Name.RichardLawrence;
            }
            throw new Exception("Cannot unmarshal type Name");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Name)untypedValue;
            if (value == Name.RichardLawrence)
            {
                serializer.Serialize(writer, "Richard Lawrence");
                return;
            }
            throw new Exception("Cannot marshal type Name");
        }

        public static readonly NameConverter Singleton = new NameConverter();
    }

    internal class CreatedByTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(CreatedByType) || t == typeof(CreatedByType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "user")
            {
                return CreatedByType.User;
            }
            throw new Exception("Cannot unmarshal type CreatedByType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (CreatedByType)untypedValue;
            if (value == CreatedByType.User)
            {
                serializer.Serialize(writer, "user");
                return;
            }
            throw new Exception("Cannot marshal type CreatedByType");
        }

        public static readonly CreatedByTypeConverter Singleton = new CreatedByTypeConverter();
    }

    internal class BackgroundColorConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(BackgroundColor) || t == typeof(BackgroundColor?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "#00a6ff")
            {
                return BackgroundColor.The00A6Ff;
            }
            throw new Exception("Cannot unmarshal type BackgroundColor");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (BackgroundColor)untypedValue;
            if (value == BackgroundColor.The00A6Ff)
            {
                serializer.Serialize(writer, "#00a6ff");
                return;
            }
            throw new Exception("Cannot marshal type BackgroundColor");
        }

        public static readonly BackgroundColorConverter Singleton = new BackgroundColorConverter();
    }

    internal class ColorConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Color) || t == typeof(Color?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "#ff7b00")
            {
                return Color.Ff7B00;
            }
            throw new Exception("Cannot unmarshal type Color");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Color)untypedValue;
            if (value == Color.Ff7B00)
            {
                serializer.Serialize(writer, "#ff7b00");
                return;
            }
            throw new Exception("Cannot marshal type Color");
        }

        public static readonly ColorConverter Singleton = new ColorConverter();
    }

    internal class BorderStyleConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(BorderStyle) || t == typeof(BorderStyle?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "normal")
            {
                return BorderStyle.Normal;
            }
            throw new Exception("Cannot unmarshal type BorderStyle");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (BorderStyle)untypedValue;
            if (value == BorderStyle.Normal)
            {
                serializer.Serialize(writer, "normal");
                return;
            }
            throw new Exception("Cannot marshal type BorderStyle");
        }

        public static readonly BorderStyleConverter Singleton = new BorderStyleConverter();
    }

    internal class FontFamilyConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(FontFamily) || t == typeof(FontFamily?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "OpenSans")
            {
                return FontFamily.OpenSans;
            }
            throw new Exception("Cannot unmarshal type FontFamily");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (FontFamily)untypedValue;
            if (value == FontFamily.OpenSans)
            {
                serializer.Serialize(writer, "OpenSans");
                return;
            }
            throw new Exception("Cannot marshal type FontFamily");
        }

        public static readonly FontFamilyConverter Singleton = new FontFamilyConverter();
    }

    internal class ShapeTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(ShapeType) || t == typeof(ShapeType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "rectangle")
            {
                return ShapeType.Rectangle;
            }
            throw new Exception("Cannot unmarshal type ShapeType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (ShapeType)untypedValue;
            if (value == ShapeType.Rectangle)
            {
                serializer.Serialize(writer, "rectangle");
                return;
            }
            throw new Exception("Cannot marshal type ShapeType");
        }

        public static readonly ShapeTypeConverter Singleton = new ShapeTypeConverter();
    }

    internal class TextAlignConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TextAlign) || t == typeof(TextAlign?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "center")
            {
                return TextAlign.Center;
            }
            throw new Exception("Cannot unmarshal type TextAlign");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TextAlign)untypedValue;
            if (value == TextAlign.Center)
            {
                serializer.Serialize(writer, "center");
                return;
            }
            throw new Exception("Cannot marshal type TextAlign");
        }

        public static readonly TextAlignConverter Singleton = new TextAlignConverter();
    }

    internal class TextAlignVerticalConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TextAlignVertical) || t == typeof(TextAlignVertical?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "middle")
            {
                return TextAlignVertical.Middle;
            }
            throw new Exception("Cannot unmarshal type TextAlignVertical");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TextAlignVertical)untypedValue;
            if (value == TextAlignVertical.Middle)
            {
                serializer.Serialize(writer, "middle");
                return;
            }
            throw new Exception("Cannot marshal type TextAlignVertical");
        }

        public static readonly TextAlignVerticalConverter Singleton = new TextAlignVerticalConverter();
    }

    internal class DatumTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(DatumType) || t == typeof(DatumType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "shape")
            {
                return DatumType.Shape;
            }
            throw new Exception("Cannot unmarshal type DatumType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (DatumType)untypedValue;
            if (value == DatumType.Shape)
            {
                serializer.Serialize(writer, "shape");
                return;
            }
            throw new Exception("Cannot marshal type DatumType");
        }

        public static readonly DatumTypeConverter Singleton = new DatumTypeConverter();
    }
}

