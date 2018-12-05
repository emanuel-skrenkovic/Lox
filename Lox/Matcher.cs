namespace Lox
{
	public abstract class Matcher : IExpressionMatcher, IStatementMatcher
	{
         public void MatchStatement(Stmt statement)
        {
            switch (statement)
            {
                case ClassStmt classStmt:
                    MatchClassStmt(classStmt);
                    break;

                case IfStmt ifStmt:
                    MatchIfStmt(ifStmt);
                    break;

                case DeclarationStmt declarationStmt:
                    MatchDeclarationStmt(declarationStmt);
                    break;

                case VariableStmt variableStmt:
                    MatchVariableStmt(variableStmt);
                    break;

                case BlockStmt blockStmt:
                    MatchBlockStmt(blockStmt);
                    break;

                case ExpressionStmt exprStmt:
                    MatchExpressionStmt(exprStmt);
                    break;

                case PrintStmt printStmt:
                    MatchPrintStmt(printStmt);
                    break; 
                
                case WhileStmt whileStmt:
                    MatchWhileStmt(whileStmt);
                    break;

                case LoopControlStmt loopControlStmt:
                    MatchLoopControlStmt(loopControlStmt);
                    break;

                case FunctionStmt functionStmt:
                    MatchFunctionStmt(functionStmt);
                    break;

                case ReturnStmt returnStmt:
                    MatchReturnStmt(returnStmt);
                    break;
            } 
        }

        public object MatchExpression(Expr expression)
        {
            switch (expression)
            {
                case GetExpr getExpr:
                    return MatchGetExpr(getExpr);

                case SetExpr setExpr:
                    return MatchSetExpr(setExpr);

                case VariableExpr variableExpr:
                    return MatchVariableExpr(variableExpr);

                case LogicalExpr logical:
                    return MatchLogicalExpr(logical);

                case AssignmentExpr assignment:
                    return MatchAssignExpr(assignment);

                case TernaryExpr ternary:
                    return MatchTernaryExpr(ternary);

                case BinaryExpr binary:
                    return MatchBinaryExpr(binary);

                case CallExpr call:
                    return MatchCallExpr(call);

                case UnaryExpr unary:
                    return MatchUnaryExpr(unary);

                case GroupingExpr grouping:
                    return MatchGroupingExpr(grouping);

                case LiteralExpr literal:
                    return MatchLiteralExpr(literal);

                case FunctionExpr function:
                    return MatchFunctionExpr(function);

                case ThisExpr thisExpr:
                    return MatchThisExpr(thisExpr);

                case SuperExpr superExpr:
                    return MatchSuperExpr(superExpr);

				default:
					return null;
            }
        }

        protected abstract object MatchAssignExpr(AssignmentExpr assignmentExpr);
        protected abstract object MatchBinaryExpr(BinaryExpr binaryExpr);
        protected abstract object MatchCallExpr(CallExpr callExpr);
        protected abstract object MatchFunctionExpr(FunctionExpr functionExpr);
        protected abstract object MatchGetExpr(GetExpr getExpr);
        protected abstract object MatchGroupingExpr(GroupingExpr groupingExpr);
        protected abstract object MatchLiteralExpr(LiteralExpr literalExpr);
        protected abstract object MatchLogicalExpr(LogicalExpr logicalExpr);
        protected abstract object MatchSetExpr(SetExpr setExpr);
        protected abstract object MatchSuperExpr(SuperExpr superExpr);
        protected abstract object MatchTernaryExpr(TernaryExpr ternaryExpr);
        protected abstract object MatchThisExpr(ThisExpr thisExpr);
        protected abstract object MatchUnaryExpr(UnaryExpr unaryExpr);
		protected abstract object MatchVariableExpr(VariableExpr variableExpr);


        protected abstract void MatchClassStmt(ClassStmt classStmt);
        protected abstract void MatchIfStmt(IfStmt ifStmt);
        protected abstract void MatchDeclarationStmt(DeclarationStmt declarationStmt);
        protected abstract void MatchVariableStmt(VariableStmt variableStmt);
        protected abstract void MatchBlockStmt(BlockStmt blockStmt);
        protected abstract void MatchExpressionStmt(ExpressionStmt exprStmt);
        protected abstract void MatchPrintStmt(PrintStmt printStmt);
        protected abstract void MatchWhileStmt(WhileStmt whileStmt);
        protected abstract void MatchLoopControlStmt(LoopControlStmt loopControlStmt);
        protected abstract void MatchFunctionStmt(FunctionStmt functionStmt);
        protected abstract void MatchReturnStmt(ReturnStmt returnStmt);
    }
}
