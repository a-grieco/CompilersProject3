.assembly extern mscorlib {}
.assembly addnums {}
.class public twomethods1
{
.method public static void check(int32) 
{
.maxstack 42
ldstr "The argument to check was "
call void[mscorlib]System.Console::Write(string)
ldarg.0
call void [mscorlib]System.Console::WriteLine(int32)
ret
}
.method public static void main() 
{
.entrypoint
.maxstack 42

ldc.i4 12345
call void twomethods1::check(int32)
ldstr "Back in main"
call void [mscorlib]System.Console::WriteLine(string)
ret
}
}
