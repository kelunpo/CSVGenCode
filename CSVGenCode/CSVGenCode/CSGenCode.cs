/*
author: jiepengtan(davidjiepengtan@qq.com)
date    20170519
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif
namespace CSVGenCode {

    public static class StringExternal {
        public static String ToMacro(this String name) {
            StringBuilder result = new StringBuilder();
            if (name != null && name.Length > 0) {
                for (int i = 0; i < name.Length - 1; i++) {
                    var s = name[i];
                    var sp = name[i + 1];
                    if (char.IsLower(s) && char.IsUpper(sp)) {
                        result.Append("_");
                    }
                    result.Append(s);
                }
                result.Append(name[name.Length - 1]);
            }
            return result.ToString();
        }

        public static String ToBigCamel(this String name) {
            StringBuilder result = new StringBuilder();
            if (string.IsNullOrEmpty(name)) {
                return "";
            } else if (!name.Contains("_")) {
                return name.Substring(0, 1).ToUpper() + name.Substring(1);
            }
            var camels = name.Split('_');
            foreach (var camel in camels) {
                if (string.IsNullOrEmpty(camel)) {
                    continue;
                }
                result.Append(camel.Substring(0, 1).ToUpper());
                result.Append(camel.Substring(1).ToLower());
            }
            return result.ToString();
        }
    }
    public class CsvParser {
        private const char CommaCharacter = ',';
        private const char QuoteCharacter = '"';

        #region Nested types

        private abstract class ParserState {
            public static readonly LineStartState LineStartState = new LineStartState();
            public static readonly ValueStartState ValueStartState = new ValueStartState();
            public static readonly ValueState ValueState = new ValueState();
            public static readonly QuotedValueState QuotedValueState = new QuotedValueState();
            public static readonly QuoteState QuoteState = new QuoteState();

            public abstract ParserState AnyChar(char ch, ParserContext context);
            public abstract ParserState Comma(ParserContext context);
            public abstract ParserState Quote(ParserContext context);
            public abstract ParserState EndOfLine(ParserContext context);
        }

        private class LineStartState : ParserState {
            public override ParserState AnyChar(char ch, ParserContext context) {
                context.AddChar(ch);
                return ValueState;
            }

            public override ParserState Comma(ParserContext context) {
                context.AddValue();
                return ValueStartState;
            }

            public override ParserState Quote(ParserContext context) {
                return QuotedValueState;
            }

            public override ParserState EndOfLine(ParserContext context) {
                context.AddLine();
                return LineStartState;
            }
        }

        private class ValueStartState : LineStartState {
            public override ParserState EndOfLine(ParserContext context) {
                context.AddValue();
                context.AddLine();
                return LineStartState;
            }
        }

        private class ValueState : ParserState {
            public override ParserState AnyChar(char ch, ParserContext context) {
                context.AddChar(ch);
                return ValueState;
            }

            public override ParserState Comma(ParserContext context) {
                context.AddValue();
                return ValueStartState;
            }

            public override ParserState Quote(ParserContext context) {
                context.AddChar(QuoteCharacter);
                return ValueState;
            }

            public override ParserState EndOfLine(ParserContext context) {
                context.AddValue();
                context.AddLine();
                return LineStartState;
            }
        }

        private class QuotedValueState : ParserState {
            public override ParserState AnyChar(char ch, ParserContext context) {
                context.AddChar(ch);
                return QuotedValueState;
            }

            public override ParserState Comma(ParserContext context) {
                context.AddChar(CommaCharacter);
                return QuotedValueState;
            }

            public override ParserState Quote(ParserContext context) {
                return QuoteState;
            }

            public override ParserState EndOfLine(ParserContext context) {
                context.AddChar('\r');
                context.AddChar('\n');
                return QuotedValueState;
            }
        }

        private class QuoteState : ParserState {
            public override ParserState AnyChar(char ch, ParserContext context) {
                //undefined, ignore "
                context.AddChar(ch);
                return QuotedValueState;
            }

            public override ParserState Comma(ParserContext context) {
                context.AddValue();
                return ValueStartState;
            }

            public override ParserState Quote(ParserContext context) {
                context.AddChar(QuoteCharacter);
                return QuotedValueState;
            }

            public override ParserState EndOfLine(ParserContext context) {
                context.AddValue();
                context.AddLine();
                return LineStartState;
            }
        }

        private class ParserContext {
            private readonly StringBuilder _currentValue = new StringBuilder();
            private readonly List<string[]> _lines = new List<string[]>();
            private readonly List<string> _currentLine = new List<string>();

            public ParserContext() {
                MaxColumnsToRead = 1000;
            }

            public int MaxColumnsToRead { get; set; }

            public void AddChar(char ch) {
                _currentValue.Append(ch);
            }

            public void AddValue() {
                if (_currentLine.Count < MaxColumnsToRead)
                    _currentLine.Add(_currentValue.ToString());
                _currentValue.Remove(0, _currentValue.Length);
            }

            public void AddLine() {
                _lines.Add(_currentLine.ToArray());
                _currentLine.Clear();
            }

            public List<string[]> GetAllLines() {
                if (_currentValue.Length > 0) {
                    AddValue();
                }
                if (_currentLine.Count > 0) {
                    AddLine();
                }
                return _lines;
            }
        }

        #endregion

        public bool TrimTrailingEmptyLines { get; set; }
        public int MaxColumnsToRead { get; set; }

        public string[][] Parse(TextReader reader) {
            var context = new ParserContext();
            if (MaxColumnsToRead != 0)
                context.MaxColumnsToRead = MaxColumnsToRead;

            ParserState currentState = ParserState.LineStartState;
            string next;
            while (( next = reader.ReadLine() ) != null) {
                foreach (char ch in next) {
                    switch (ch) {
                        case CommaCharacter:
                            currentState = currentState.Comma(context);
                            break;
                        case QuoteCharacter:
                            currentState = currentState.Quote(context);
                            break;
                        default:
                            currentState = currentState.AnyChar(ch, context);
                            break;
                    }
                }
                currentState = currentState.EndOfLine(context);
            }
            List<string[]> allLines = context.GetAllLines();
            if (TrimTrailingEmptyLines && allLines.Count > 0) {
                bool isEmpty = true;
                for (int i = allLines.Count - 1; i >= 0; i--) {
                    // ReSharper disable RedundantAssignment
                    isEmpty = true;
                    // ReSharper restore RedundantAssignment
                    for (int j = 0; j < allLines[i].Length; j++) {
                        if (!String.IsNullOrEmpty(allLines[i][j])) {
                            isEmpty = false;
                            break;
                        }
                    }
                    if (!isEmpty) {
                        if (i < allLines.Count - 1)
                            allLines.RemoveRange(i + 1, allLines.Count - i - 1);
                        break;
                    }
                }
                if (isEmpty)
                    allLines.RemoveRange(0, allLines.Count);
            }
            return allLines.ToArray();
        }

        public static string[][] Parse(string input) {
            CsvParser parser = new CsvParser();

            using (StringReader reader = new StringReader(input)) {
                return parser.Parse(reader);
            }
        }
    }

    public class Debug {
        static public void LogError(object message) {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(message);
#else
            Console.WriteLine(message);
#endif
        }
        static public void Log(object message) {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(message);
#else
            Console.WriteLine(message);
#endif
        }
    }
    public class Util {

        public static void Walk(string path, string exts, System.Action<string> callback, bool isEditor = false) {
            bool isAll = string.IsNullOrEmpty(exts) || exts == "*" || exts == "*.*";
            string[] extList = exts.Replace("*", "").Split('|');

            if (Directory.Exists(path)) {
                // 如果选择的是文件夹

                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(file => {
                    if (isAll)
                        return true;
                    foreach (var ext in extList) {
                        if (file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) {
                            return true;
                        }
                    }
                    return false;
                }).ToArray();

                foreach (var item in files) {
                    if (callback != null) {
                        callback(item);
                    }
                }
                if (isEditor) {
#if UNITY_EDITOR
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
#endif
                }
            } else {
                if (isAll) {
                    if (callback != null) {
                        callback(path);
                    }
                } else {
                    // 如果选择的是文件
                    foreach (var ext in extList) {
                        if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) {
                            if (callback != null) {
                                callback(path);
                            }
                        }
                    }
                }
                if (isEditor) {
#if UNITY_EDITOR
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
#endif
                }
            }
        }
    }
    public class CodeContent {
        public string structStr;
        public string headStr;
        public string implStr;
        public string mgrHeadStr;
        public string mgrImplStr;
    }
    public class SReplaceInfo {
        public string TempletPath = "";
        public string KeyWord = "";
        public string SavePath = "";
        public SReplaceInfo(string KeyWord, string TempletPath, string SavePath) {
            this.TempletPath = TempletPath;
            this.KeyWord = KeyWord;
            this.SavePath = SavePath;
        }
    }
    public class CSV2CppConst {
        public static Dictionary<string, string> Type2FuncNameMap = new Dictionary<string, string>();
        public static Dictionary<string, string> Type2CodeTypeMap = new Dictionary<string, string>();
        public static string StructNamePrefix = "";
        public static string ClassNamePrefix = "";
        public static string CSVType2FuncName(string typeName) {
            if (Type2FuncNameMap.ContainsKey(typeName)) {
                return Type2FuncNameMap[typeName];
            } else {
                Debug.LogError("GetConvertFunc ErrorType " + typeName);
                return "";
            }
        }
        public static string CSVType2CodeType(string typeName) {
            if (Type2CodeTypeMap.ContainsKey(typeName)) {
                return Type2CodeTypeMap[typeName];
            } else {
                Debug.LogError("CSVType2CodeTypeMap ErrorType " + typeName);
                return typeName;
            }
        }
    }

    public class TempletInfo {
        public string fileStr;
        public List<TempletClsInfo> clsLst = new List<TempletClsInfo>();
        public List<string> attrLst = new List<string>();
    }
    public class TempletClsInfo {
        public string clsStr;
        public List<string> attrLst = new List<string>();
    }
    public class GenTypeInfo {
        public string FileName;
        public string KeyTypeName;
        public string KeyName;
        public string StructName;
        public string ClsName;
        public CodeContent _info = new CodeContent();
        public List<ColInfo> colInfo = new List<ColInfo>();

        public void Init() {
            FileName = FileName.ToBigCamel();
            StructName = CSV2CppConst.StructNamePrefix + FileName;
            ClsName = CSV2CppConst.ClassNamePrefix + FileName;
        }
    }
    public class ColInfo {
        public string AttriCommment;
        public string AttriTypeName;
        public string AttriName;
        //gen by rule
        public string AttriType2FuncName;
        public void Init() {
            AttriType2FuncName = CSV2CppConst.CSVType2FuncName(AttriTypeName);
            AttriTypeName = CSV2CppConst.CSVType2CodeType(AttriTypeName);
        }
    }
    public class SpliteInfo {
        public string content;//Exclude Tag
        public string RawStr;//Include Tag
        public int startIdx;
        public int endIdx;
    }
    public class CSVToCppCodeGen {
        private string OutputPath = "";
        private string clsTagBegin = "#Begin_Replace_Tag_Class";
        private string clsTagEnd = "#End_Replace_Tag_Class";
        private string attrTagBegin = "#Begin_Replace_Tag_Attri";
        private string attrTagEnd = "#End_Replace_Tag_Attri";
        private List<GenTypeInfo> infos = new List<GenTypeInfo>();
        private int totalFileNum = 0;

        private GenTypeInfo curTypeInfo;//当前处理的类的信息
        private ColInfo curColInfo;//当前处理的类的信息
        private bool isSkipEnter = true;//是否跳过换行符
        private int skipNum;//跳过的字数
        string ReplaceClsInfo(string info, GenTypeInfo typeInfo) {
            info = info.Replace("#FileName", typeInfo.FileName);
            info = info.Replace("#KeyTypeName", typeInfo.KeyTypeName);
            info = info.Replace("#KeyName", typeInfo.KeyName);
            info = info.Replace("#StructName", typeInfo.StructName);
            info = info.Replace("#ClsName", typeInfo.ClsName);
            return info;
        }
        string ReplaceAttrInfo(string info, GenTypeInfo typeInfo, ColInfo colInfo) {
            info = info.Replace("#FileName", typeInfo.FileName);
            info = info.Replace("#KeyTypeName", typeInfo.KeyTypeName);
            info = info.Replace("#KeyName", typeInfo.KeyName);
            info = info.Replace("#StructName", typeInfo.StructName);
            info = info.Replace("#ClsName", typeInfo.ClsName);

            info = info.Replace("#AttriName", colInfo.AttriName);
            info = info.Replace("#AttriType2FuncName", colInfo.AttriType2FuncName);
            info = info.Replace("#AttriTypeName", colInfo.AttriTypeName);
            info = info.Replace("#AttriCommment", colInfo.AttriCommment);
            return info;
        }



        string GetOutPath(string path) {
            var fileName = Path.GetFileName(path);
            return Path.Combine(OutputPath, fileName);
        }
        public void GenCode(string InputPath, string OutputPath, string TempletPath, string ConfigFile) {
            ReadConfig(ConfigFile);
            this.OutputPath = OutputPath;
            Util.Walk(InputPath, "*.csv", ReadCsv);
            Util.Walk(TempletPath, "*.*", GenCodeFormTemplet);
            Debug.Log("Done config file" + totalFileNum);
        }

        void ReadConfig(string ConfigFile) {
            var content = File.ReadAllLines(ConfigFile);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < content.Length; i++) {
                var line = content[i];
                line = line.TrimStart();
                if (line.StartsWith("#")) {
                    continue;
                }
                sb.Append(line);
            }
            var str = sb.ToString();
            var map = str.Split(new string[] { "$$" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var km in map) {
                var strs = km.Split('=');
                if (strs.Length < 2) {
                    Debug.LogError("ConfigError!!: " + km);
                    return;
                }
                var key = strs[0].Trim();
                var val = strs[1].Trim();
                DealConfig(key, val);
            }
        }

        void DealConfig(string key, string val) {
            if (string.IsNullOrEmpty(key))
                return;
            switch (key) {
                case "Type2FuncNameMap": {
                        var map = ParseMap(val);
                        CSV2CppConst.Type2FuncNameMap = map;
                    }
                    break;
                case "Type2CodeTypeMap": {
                        var map = ParseMap(val);
                        CSV2CppConst.Type2CodeTypeMap = map;
                    }
                    break;
                case "StructNamePrefix": {
                        CSV2CppConst.StructNamePrefix = val.Trim();
                    }
                    break;
                case "ClassNamePrefix": {
                        CSV2CppConst.ClassNamePrefix = val.Trim();
                    }
                    break;
                default:
                    Debug.LogError("Error Key = " + key);
                    break;
            }
        }

        Dictionary<string, string> ParseMap(string val) {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            var lines = val.Split(';');
            foreach (var item in lines) {
                if (string.IsNullOrEmpty(item))
                    continue;
                var strs = item.Split(':');
                if (strs.Length < 2) {
                    Debug.LogError("ConfigError!!: " + val);
                    return ret;
                }
                ret.Add(strs[0].Trim(), strs[1].Trim());
            }
            return ret;
        }
        void ReadCsv(string path) {
            var csvText = File.ReadAllText(path, Encoding.UTF8);
            var fileName = Path.GetFileName(path);
            fileName = fileName.Split('.')[0];
            Debug.Log(fileName);
            totalFileNum++;
            if (string.IsNullOrEmpty(csvText))
                return;
            string[][] grid = CsvParser.Parse(csvText);
            if (grid.Length < 3) {
                Debug.LogError("CSV2Cpp CodeGen Csv table grid.Length < 3");
                return;
            }
            int len = grid[0].Length;
            foreach (var row in grid) {
                if (row.Length != len) {
                    Debug.LogError(string.Format("{2} csv parse error not each row's len is the same row.Length{0} != titleLen{1}", row.Length, len, path));
                }
            }
            var info = new GenTypeInfo();
            int __idx = 0;
            var comment = grid[__idx++];
            var attrName = grid[__idx++];
            var attrType = grid[__idx++];
            info.FileName = fileName;
            info.KeyTypeName = CSV2CppConst.CSVType2CodeType(attrType[0]);
            info.KeyName = attrName[0];
            for (int i = 0; i < len; i++) {
                var col = new ColInfo();
                col.AttriCommment = comment[i];
                col.AttriName = attrName[i];
                col.AttriTypeName = attrType[i];
                col.Init();
                info.colInfo.Add(col);
            }
            info.Init();
            infos.Add(info);
            //Print(grid);
            return;
        }
        private void Print(string[][] grid) {
            StringBuilder sb = new StringBuilder();
            foreach (var row in grid) {
                foreach (var col in row) {
                    sb.Append(col + " ");
                }
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }
        void GenCodeFormTemplet(string path) {
            var content = File.ReadAllText(path);
            skipNum = GetSkipNum(content);
            var ret = ReplaceFile(content);
            WriteTo(ret, GetOutPath(path));
        }
        int GetSkipNum(string content) {
            if (!isSkipEnter)
                return 0;
            if (content.Contains("\r\n")) {
                return 2;
            } else {
                return 1;
            }
        }
        string ReplaceFile(string content) {
            return ReplaceClsTag(content);
        }

        //替换所有类标志
        string ReplaceClsTag(string content) {
            var ret = Replace(content, clsTagBegin, clsTagEnd, (str) => {
                //正对一个类标志 需要遍历所有的类 
                StringBuilder sb = new StringBuilder();
                foreach (var info in infos) {
                    curTypeInfo = info;
                    var _replaceStr = ReplaceAttrTag(str, curTypeInfo);
                    _replaceStr = ReplaceClsInfo(_replaceStr, curTypeInfo);
                    sb.Append(_replaceStr);
                }
                return sb.ToString();
            });
            return ret;
        }

        //替换所有属性标志
        string ReplaceAttrTag(string content, GenTypeInfo typeInfo) {
            var ret = Replace(content, attrTagBegin, attrTagEnd, (str) => {
                //正对一个属性标志 需要遍历所有的属性 
                StringBuilder sb = new StringBuilder();
                foreach (var colInfo in typeInfo.colInfo) {
                    curColInfo = colInfo;
                    var _replaceStr = ReplaceAttrInfo(str, curTypeInfo, curColInfo);
                    sb.Append(_replaceStr);
                }
                return sb.ToString();
            });
            return ret;
        }
        private int logNum = 0;
        string Replace(string content, string tagBegin, string tagEnd, Func<string, string> DealFunc) {
            var infos = Split(content, tagBegin, tagEnd);
            foreach (var info in infos) {
                var FinalStr = DealFunc(info.content);
                if (!content.Contains(info.RawStr)) {
                    logNum++;
                    if (logNum < 5) {
                        Debug.LogError("Str=" + content);
                        Debug.LogError("SubStr=" + info.RawStr);
                    }
                }
                content = content.Replace(info.RawStr, FinalStr);
            }
            return content;
        }
        List<SpliteInfo> Split(string content, string begTag, string endTag) {
            var allCls = content.Split(new string[] { begTag }, StringSplitOptions.None);
            List<SpliteInfo> ret = new List<SpliteInfo>();
            int startIdx = 0;
            int endIdx = 0;
            while (endIdx < content.Length) {
                startIdx = content.IndexOf((string)begTag, endIdx);
                if (startIdx == -1)
                    break;
                SpliteInfo info = new SpliteInfo();
                endIdx = content.IndexOf((string)endTag, startIdx);
                info.startIdx = startIdx;
                info.content = content.Substring(startIdx + begTag.Length + skipNum, endIdx - ( startIdx + begTag.Length ) - skipNum);
                info.endIdx = endIdx + endTag.Length;
                info.RawStr = content.Substring(startIdx, endIdx + endTag.Length - startIdx);
                ret.Add(info);
            }
            return ret;
        }
        void WriteTo(string content, string outPath) {
            var outDir = Path.GetDirectoryName(outPath);
            if (!Directory.Exists(outDir)) {
                Directory.CreateDirectory(outDir);
            }
            File.WriteAllText(outPath, content, Encoding.UTF8);
        }
    }

}

#if UNITY_EDITOR
//生成CPP 代码
public class EditorCSVToCpp {
    static string TempletPath = Path.Combine(Application.dataPath, "../CSVToCpp/Templet");
    static string InputDir = Path.Combine(Application.dataPath, "../CSVToCpp/Input");
    static string OutputDir = Path.Combine(Application.dataPath, "../CSVToCpp/Output");
    static string ConfigFile = Path.Combine(Application.dataPath, "../CSVToCpp/KeywordMapRule.txt");
    [MenuItem("Tools/CSVToCpp")]
    public static void MoveUnNeedGraphicRaycaster() {
        var gener = new CSVToCpp.CSVToCppCodeGen();
        gener.GenCode(InputDir, OutputDir, TempletPath, ConfigFile);
    }
}
#else

class Program {
    static string TempletPath = Path.Combine(Environment.CurrentDirectory, "../../CSVToCpp/Templet");
    static string InputDir = Path.Combine(Environment.CurrentDirectory, "../../CSVToCpp/Input");
    static string OutputDir = Path.Combine(Environment.CurrentDirectory, "../../CSVToCpp/Output");
    static string ConfigFile = Path.Combine(Environment.CurrentDirectory, "../../CSVToCpp/KeywordMapRule.txt");

    static void Main(string[] args) {
        CSVGenCode.Debug.Log(TempletPath);
        var gener = new CSVGenCode.CSVToCppCodeGen();
        gener.GenCode(InputDir, OutputDir, TempletPath, ConfigFile);
    }
}
#endif
