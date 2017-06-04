.assembly extern mscorlib {}
.assembly addnums {}
.class public hello
{
.method public static void main() cil managed 
{
.entrypoint
.maxstack 42

ldstr "Hello, World"
call void [mscorlib]System.Console::WriteLine(string)
ret
}
}
