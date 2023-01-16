using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RosettaUI.Example
{
    public static class SyntaxHighlighter
    {
        private static readonly Dictionary<string, List<string>> Patterns = new()
        {
            ["type"] = new()
            {
                "Range",
                "Multiline",
                "NonReorderable",
                "NonSerialized",
                "((?<!\\.)List)",
                "T",
                nameof(Enumerable),
                nameof(Vector2),
                nameof(Color),
                nameof(Screen),
                nameof(Debug),
                nameof(UI),
                nameof(UICustom),
                "ElementCreationFuncScope",
                "PropertyOrFieldsScope",
                nameof(UICustom.PropertyOrFieldLabelModifierScope),
                nameof(MinMax),
                nameof(ListViewOption),
                nameof(IElementCreator),
                nameof(Element),
                nameof(LabelElement)
            },
            ["method"] = new()
            {
                nameof(ToString),
                nameof(string.ToUpper),
                nameof(string.ToLower),
                nameof(Enumerable.Range),
                nameof(Enumerable.Select),
                nameof(Debug.Log),
                nameof(UI.Field),
                nameof(UI.FieldReadOnly),
                nameof(UI.Slider),
                nameof(UI.MinMaxSlider),
                nameof(UI.List),
                nameof(UI.Fold),
                nameof(UI.WindowLauncher),
                nameof(UI.Window),
                nameof(UI.Label),
                nameof(UI.Row),
                nameof(UI.Button),
                nameof(UI.Box),
                nameof(UI.FieldIfObjectFound),
                nameof(UI.DynamicElementIf),
                nameof(UI.DynamicElementOnStatusChanged),
                nameof(UI.Dropdown),
                nameof(UI.TextArea),
                nameof(MinMax.Create),
                nameof(ElementExtensionsMethodChain.SetEnable),
                nameof(ElementExtensionsMethodChain.SetInteractable),
                nameof(ElementExtensionsMethodChain.SetWidth),
                nameof(ElementExtensionsMethodChain.SetHeight),
                nameof(ElementExtensionsMethodChain.SetColor),
                nameof(ElementExtensionsMethodChain.SetBackgroundColor),
                nameof(ElementExtensionsMethodChain.SetOpenFlag),
                nameof(ElementExtensionsMethodChain.Open),
                nameof(ElementExtensionsMethodChain.Close),
                nameof(ElementExtensionsMethodChain.RegisterUpdateCallback),
                nameof(ElementExtensionsMethodChain.RegisterValueChangeCallback),
                nameof(IElementCreator.CreateElement)
            },
            ["keyword"] = new()
            {
                "public",
                "protected",
                "private",
                "class",
                "struct",
                "return",
                "void",
                "bool",
                "int",
                "float",
                "new",
                "nameof",
                "true",
                "false",
                "null",
                "var",
                "using",
            },
            ["symbol"] = new()
            {
                @"[{}()=;,+\-*/<>|\[\]]+",
            },
            ["digit"] = new()
            {
                @"(?<![a-zA-Z_])[+-]?[0-9]+\.?[0-9]?(([eE][+-]?)?[0-9]+)?f?"
            },
            ["str"] = new()
            {
                "(\\$?\"[^\"\\n]*?\")"
            },
            ["parameterName"] = new()
            {
                "([a-zA-Z_]+[0-9a-zA-Z_]*:)"
            },
            ["comment"] = new()
            {
                @"/\*[\s\S]*?\*/|//.*"
            }
        };

        private static readonly Dictionary<string, string> ColorTable = new()
        {
            { "field", "#66C3CC"},
            { "type", "#C191FF" },
            { "method", "#39CC8F" },
            { "keyword", "#6C95EB" },
            { "symbol", "#BDBDBD" },
            { "digit", "#ED94C0" },
            { "str", "#C9A26D" },
            { "parameterName", "#787878" },
            { "comment", "#85C46C" },
        };

        private static Regex _regex;

        private static Regex CreateRegex()
        {
            var forwardSeparator = "(?<![0-9a-zA-Z_])";
            var backwardSeparator = "(?![0-9a-zA-Z_])";
            var format1 = "(?<{0}>({1}))";
            var format2 = string.Format("(?<{0}>{2}({1}){3})", "{0}", "{1}", forwardSeparator, backwardSeparator);

            var nameAndFormats = new[]
            {
                ("comment", format1),
                ("type", format2),
                ("method", format2),
                ("keyword", format2),
                ("symbol", format1),
                ("digit", format1),
                ("str", format1),
                ("parameterName", format1),
            };

            var patterns = nameAndFormats.Select((pair) =>
            {
                var (name, formatStr) = pair;
                return string.Format(formatStr, name, string.Join("|", Patterns[name]));
            });
            
            var combinedPattern = $"({string.Join("|", patterns)})";
            
            return new Regex(combinedPattern, RegexOptions.Compiled);
        }
        
        private static string ToColoredCode(string code, string color) => $"<color={color}>{code}</color>";

        public static void AddPattern(string name, string pattern)
        {
            Patterns[name].Add(pattern);
            _regex = null;
        }
        
        public static string Highlight(string code)
        {
            _regex ??= CreateRegex();
            return ToColoredCode(_regex.Replace(code, Evaluator), ColorTable["field"]);
            
            
            string Evaluator(Match match)
            {
                foreach (var pair in ColorTable)
                {
                    if (match.Groups[pair.Key].Success)
                    {
                        return ToColoredCode(match.Value, pair.Value);
                    }
                }

                return match.Value;
            }
        }
    }
}