.assembly extern mscorlib {}
.assembly addnums {}
.class public twomethods0
{
.method public static void check() 
{
.maxstack 42
ldstr "In the check method"
call void [mscorlib]System.Console::WriteLine(string)
ret
}
.method public static void main() 
{
.entrypoint
.maxstack 42

call void twomethods0::check()
ldstr "Back in main"
call void [mscorlib]System.Console::WriteLine(string)
ret
}
}
