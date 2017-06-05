.assembly extern mscorlib {}
.assembly addnums {}
.class public compute
{
.method public static void main() 
{
.entrypoint
.maxstack 42

.locals init(
   [0] int32 w,
   [1] int32 x
)
ldc.i4 3
ldc.i4 4
add
stloc.1
ldstr "Value of x = "
call void[mscorlib]System.Console::Write(string)
ldloc.1
call void [mscorlib]System.Console::WriteLine(int32)
ldloc.1
ldc.i4 5
sub
stloc.0
ldstr "Value of w = "
call void[mscorlib]System.Console::Write(string)
ldloc.0
call void [mscorlib]System.Console::WriteLine(int32)
ldstr "value of big expression ="
call void[mscorlib]System.Console::Write(string)
ldloc.0
ldloc.1
mul
ldc.i4 12
ldloc.0
div
add
call void [mscorlib]System.Console::WriteLine(int32)
ret
}
}
