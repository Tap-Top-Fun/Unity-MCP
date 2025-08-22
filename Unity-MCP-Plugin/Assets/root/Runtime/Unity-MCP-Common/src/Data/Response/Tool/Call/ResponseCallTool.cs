/*
┌──────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)             │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-MCP)    │
│  Copyright (c) 2025 Ivan Murzak                                  │
│  Licensed under the Apache License, Version 2.0.                 │
│  See the LICENSE file in the project root for more information.  │
└──────────────────────────────────────────────────────────────────┘
*/
using System;
using System.Collections.Generic;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.IvanMurzak.ReflectorNet.Model
{
    public class ResponseCallTool : IResponseCallTool
    {
        public virtual bool IsError { get; set; }
        public virtual List<ResponseCallToolContent> Content { get; set; } = new List<ResponseCallToolContent>();

        public ResponseCallTool() { }
        public ResponseCallTool(bool isError, List<ResponseCallToolContent> content)
        {
            IsError = isError;
            Content = content;
        }

        public static ResponseCallTool Error(Exception exception)
            => Error($"[Error] {exception?.Message}\n{exception?.StackTrace}");

        public static ResponseCallTool Error(string? message)
            => new ResponseCallTool(isError: true, new List<ResponseCallToolContent>
            {
                new ResponseCallToolContent()
                {
                    Type = "text",
                    Text = message,
                    MimeType = Consts.MimeType.TextPlain
                }
            });

        public static ResponseCallTool Success(string? message)
            => new ResponseCallTool(isError: false, new List<ResponseCallToolContent>
            {
                new ResponseCallToolContent()
                {
                    Type = "text",
                    Text = message,
                    MimeType = Consts.MimeType.TextPlain
                }
            });
    }
}
