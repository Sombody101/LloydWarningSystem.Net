// $antlr-format alignTrailingComments true, columnLimit 150, minEmptyLines 1
// $antlr-format maxEmptyLinesToKeep 1, reflowComments false, useTab false
// $antlr-format allowShortRulesOnASingleLine false, allowShortBlocksOnASingleLine true
// $antlr-format alignSemicolons hanging, alignColons hanging

parser grammar LScriptParser;

options {
    tokenVocab = LScriptLexer;
}

script
    : statement*
    ;

statement
    : line
    | (routineDefinition | routineCall) ';'*
    ;

variableAssignment
    : Identifier variableDefinition
    ;

variableDeclaration
    : Const? Local? Identifier
    ;

variableDefinition
    : '=' expression
    ;

routineDefinition
    : Sub Identifier '(' parameterList? ')' block
    ;

routineCall
    : Identifier '(' (expression (',' expression)*)? ')'
    ;

block
    : '{' line* '}'
    | Arrow line
    ;

expression
    : Identifier         # identifierExpression
    | StringLiteral      # stringLiteralExpression
    | variableAssignment # variableAssignmentExpression
    | routineDefinition  # routineDefinitionExpression
    | routineCall        # routineCallExpression
    | block              # blockExpression
    ;

line
    : expression ';'*?
    ;

parameterList
    : Identifier (',' Identifier)*
    ;