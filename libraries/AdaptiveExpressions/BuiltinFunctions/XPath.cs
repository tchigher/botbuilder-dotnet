﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check XML for nodes or values that match an XPath (XML Path Language) expression, and return the matching nodes or values.
    /// An XPath expression (referred to as XPath) helps you navigate an XML document structure so that you can select nodes or compute values in the XML content.
    /// </summary>
    public class XPath : ExpressionEvaluator
    {
        public XPath()
            : base(ExpressionType.XPath, Evaluator(), ReturnType.Object, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => EvalXPath(args[0], args[1]));
        }

        private static (object, string) EvalXPath(object xmlObj, object xpath)
        {
            object value = null;
            object result = null;
            string error = null;
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlObj.ToString());
            }
            catch
            {
                error = "not valid xml input";
            }

            if (error == null)
            {
                var nav = doc.CreateNavigator();
                var strExpr = xpath.ToString();
                var nodeList = new List<string>();
                try
                {
                    value = nav.Evaluate(strExpr);
                    if (value is IEnumerable)
                    {
                        var iterNodes = nav.Select(strExpr);
                        while (iterNodes.MoveNext())
                        {
                            var nodeType = (System.Xml.XmlNodeType)iterNodes.Current.NodeType;
                            var name = iterNodes.Current.Name;
                            var nameSpaceURI = iterNodes.Current.NamespaceURI.ToString();
                            var node = doc.CreateNode(nodeType, name, nameSpaceURI);
                            node.InnerText = iterNodes.Current.Value;
                            nodeList.Add(node.OuterXml.ToString());
                        }

                        if (nodeList.Count == 0)
                        {
                            error = "there is no matched nodes in the xml";
                        }
                    }
                }
                catch
                {
                    error = $"cannot evaluate the xpath query expression: {xpath.ToString()}";
                }

                if (error == null)
                {
                    if (nodeList.Count >= 1)
                    {
                        result = nodeList.ToArray();
                    }
                    else
                    {
                        result = value;
                    }
                }
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Object, ReturnType.String);
        }
    }
}
