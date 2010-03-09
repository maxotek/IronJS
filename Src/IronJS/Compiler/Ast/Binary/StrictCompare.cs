﻿using System;
using Antlr.Runtime.Tree;
using IronJS.Runtime2.Js;
using System.Collections.Generic;
using IronJS.Compiler.Tools;

#if CLR2
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

namespace IronJS.Compiler.Ast {
	public class StrictCompare : Node {
		public INode Left { get { return Children[0]; } }
		public INode Right { get { return Children[1]; } }
		public ExpressionType Op { get; protected set; }

		public StrictCompare(INode left, INode right, ExpressionType op, ITree node)
			: base(NodeType.StrictCompare, node) {
			Children = new[] { left, right };
			Op = op;
		}

		public override Type Type {
			get {
				return IjsTypes.Boolean;
			}
		}

		public override INode Analyze(Stack<Function> stack) {
			base.Analyze(stack);

			AnalyzeTools.IfIdentifierAssignedFrom(Left, Right);
			AnalyzeTools.IfIdentifierAssignedFrom(Right, Left);

			return this;
		}
	}
}