.assembly extern mscorlib {}
.assembly addnums {}
.class public twomethods1
{
.method public void check( x) cil managed 
{
.maxstack 42
ldstr "The argument to check was "
call void[mscorlib]System.Console::Write(string)
ldloc.-1
call void [mscorlib]System.Console::WriteLine(int32)
ret
}
.method public static void main() cil managed 
{
.entrypoint
.maxstack 42

ldc.i4.s 12345
ldstr "Back in main"
call void [mscorlib]System.Console::WriteLine(string)
ret
}
}
