﻿using System;
using System.IO;
using System.Net.Http;
using DreadBot;
using Newtonsoft.Json.Linq;
using File = System.IO.File;

namespace TelegramDataEnrichment.Sessions
{
    public class DatumId
    {
        private readonly string _value;

        public DatumId(string datumId)
        {
            _value = datumId;
        }

        public override bool Equals(object obj)
        {
            if (obj is DatumId d)
            {
                return _value.Equals(d._value);
            }
            throw new ArgumentException("obj is not a DatumId object.");
        }

        public override int GetHashCode()
        {
            return (_value != null ? _value.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return _value;
        }
    }
    
    public abstract class Datum
    {
        public readonly DatumId DatumId;  // This wants to be an external unique ID

        protected Datum(DatumId datumId)
        {
            DatumId = datumId;
        }

        public abstract Result<Message> Post(long chatId, InlineKeyboardMarkup keyboard);

        public override bool Equals(object obj)
        {
            if (obj is Datum d)
            {
                return DatumId.Equals(d.DatumId);
            }
            throw new ArgumentException("obj is not a Datum object.");
        }

        public override int GetHashCode()
        {
            return (DatumId != null ? DatumId.GetHashCode() : 0);
        }

        public static FileDatum FromFile(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            var datumId = new DatumId(Path.GetFileName(fileName));
            switch (ext)
            {
                case ".txt":
                    return new TextFileDatum(datumId, fileName);
                case ".png":
                case ".jpg":
                case ".jpeg":
                    return  new ImageDatum(datumId, fileName);
                case ".mp4":
                case ".gif":
                    return new AnimationDatum(datumId, fileName);
                default:
                    return new DocumentDatum(datumId, fileName);
            }
        }

        public static JsonDatum FromJson(DatumId datumId, JObject jsonData)
        {
            return new JsonDatum(datumId, jsonData);
        }

        public static Datum FromJson(DatumId datumId, JObject jsonData, string textKey)
        {
            return new JsonTextDatum(datumId, jsonData, textKey);
        }
    }

    public abstract class FileDatum : Datum
    {
        public readonly string FileName;

        protected FileDatum(DatumId datumId, string fileName) : base(datumId)
        {
            FileName = fileName;
        }
    }

    public class TextFileDatum : FileDatum
    {
        private readonly string _text;
        
        public TextFileDatum(DatumId datumId, string fileName) : base(datumId, fileName)
        {
            _text = File.ReadAllText(fileName);
        }

        public override Result<Message> Post(long chatId, InlineKeyboardMarkup keyboard)
        {
            return Methods.sendMessage(chatId, _text, keyboard: keyboard);
        }
    }

    public class ImageDatum : FileDatum
    {
        public ImageDatum(DatumId datumId, string imagePath) : base(datumId, imagePath)
        {
        }

        public override Result<Message> Post(long chatId, InlineKeyboardMarkup keyboard)
        {
            var stream = File.OpenRead(FileName);
            var imageContent = new StreamContent(stream);
            return Methods.sendPhoto(chatId, imageContent, FileName,"", keyboard: keyboard);
        }
    }

    public class AnimationDatum : FileDatum
    {
        public AnimationDatum(DatumId datumId, string documentPath) : base(datumId, documentPath)
        {
        }

        public override Result<Message> Post(long chatId, InlineKeyboardMarkup keyboard)
        {
            var stream = File.OpenRead(FileName);
            return Methods.sendAnimation(chatId, stream, FileName, keyboard: keyboard);
        }
    }
    public class DocumentDatum : FileDatum
    {
        public DocumentDatum(DatumId datumId, string documentPath)
            : base(datumId, documentPath)
        {
        }

        public override Result<Message> Post(long chatId, InlineKeyboardMarkup keyboard)
        {
            var stream = File.OpenRead(FileName);
            var docContent = new StreamContent(stream);
            return Methods.sendDocument(chatId, docContent, FileName, keyboard: keyboard);
        }
    }

    public class JsonDatum : Datum
    {
        private readonly JObject _jsonData;

        public JsonDatum(DatumId datumId, JObject jsonData) : base(datumId)
        {
            _jsonData = jsonData;
        }

        public override Result<Message> Post(long chatId, InlineKeyboardMarkup keyboard)
        {
            return Methods.sendMessage(chatId, _jsonData.ToString(), keyboard: keyboard);
        }
    }

    public class JsonTextDatum : JsonDatum
    {
        private readonly string _text;

        public JsonTextDatum(DatumId datumId, JObject jsonData, string textPath) : base(datumId, jsonData)
        {
            _text = jsonData.SelectToken(textPath)?.ToString();
        }

        public override Result<Message> Post(long chatId, InlineKeyboardMarkup keyboard)
        {
            return Methods.sendMessage(chatId, _text, keyboard: keyboard);
        }
    }
}