namespace ncc.AST
{
	class STMT {}

	class EXPR : STMT {}

	class BLOCK : STMT 
	{
		STMT[] statements;
	}

	class EXPR_LIST : EXPR {}
}