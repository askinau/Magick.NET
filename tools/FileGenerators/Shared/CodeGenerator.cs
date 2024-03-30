﻿// Copyright Dirk Lemstra https://github.com/dlemstra/Magick.NET.
// Licensed under the Apache License, Version 2.0.

using System;
using System.CodeDom.Compiler;
using System.IO;

namespace FileGenerator;

public abstract class CodeGenerator
{
    private IndentedTextWriter _writer = default!;

    protected CodeGenerator()
    {
    }

    protected CodeGenerator(CodeGenerator parent)
        => _writer = parent._writer;

    protected int Indent
    {
        get => _writer.Indent;
        set => _writer.Indent = value;
    }

    public void CloseWriter()
    {
        _writer.InnerWriter.Dispose();
        _writer.Dispose();
    }

    public void CreateWriter(string fileName)
    {
        Console.WriteLine("Creating: " + fileName);

        var streamWriter = new StreamWriter(fileName);
        _writer = new IndentedTextWriter(streamWriter, "    ");
    }

    protected void Write(char value)
        => _writer.Write(value);

    protected void Write(int value)
        => _writer.Write(value);

    protected void Write(string? value)
        => _writer.Write(value);

    protected void WriteElse(string action)
    {
        WriteLine("else");
        Indent++;
        WriteLine(action);
        Indent--;
    }

    protected void WriteEndColon()
    {
        Indent--;
        WriteLine("}");
    }

    protected void WriteIf(string condition, string action)
    {
        WriteLine("if (" + condition + ")");
        Indent++;
        WriteLine(action);
        Indent--;
    }

    protected void WriteLine()
    {
        var indent = Indent;
        Indent = 0;
        _writer.WriteLine();
        Indent = indent;
    }

    protected void WriteLine(string value)
        => _writer.WriteLine(value);

    protected void WriteQuantumType()
    {
        WriteLine("#if Q8");
        WriteLine("using QuantumType = System.Byte;");
        WriteLine("#elif Q16");
        WriteLine("using QuantumType = System.UInt16;");
        WriteLine("#elif Q16HDRI");
        WriteLine("using QuantumType = System.Single;");
        WriteLine("#else");
        WriteLine("#error Not implemented!");
        WriteLine("#endif");
        WriteLine();
    }

    protected void WriteStart(string namespaceName)
    {
        WriteLine("// Copyright Dirk Lemstra https://github.com/dlemstra/Magick.NET.");
        WriteLine("// Licensed under the Apache License, Version 2.0.");
        WriteLine("// <auto-generated/>");
        WriteLine("#nullable enable");
        WriteLine();
        WriteUsing();
        WriteLine($"namespace {namespaceName};");
        WriteLine();
    }

    protected void WriteStartColon()
    {
        WriteLine("{");
        Indent++;
    }

    protected virtual void WriteUsing()
    {
    }
}
