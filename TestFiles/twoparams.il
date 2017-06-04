.assembly extern mscorlib {}
.assembly addnums {}
.class public twoparams
{
.method public static void outTwoInts( p1  p2) cil managed 
{
.maxstack 42
ldloc.-1
call void[mscorlib]System.Console::Write(int32)
ldstr " is the value of the first parameter"
call void [mscorlib]System.Console::WriteLine(string)
ldloc.-1
call void[mscorlib]System.Console::Write(int32)
ldstr " is the value of the second parameter"
call void [mscorlib]System.Console::WriteLine(string)
ret
}
.method public static void main() cil managed 
{
.entrypoint
.maxstack 42

.locals init(
   [0] int32 w,
   [1] int32 x,
   [2] int32 y
)
ldstr "TCCL test of method with two parameters test"
call void [mscorlib]System.Console::WriteLine(string)
ldc.i4.s 3
ldc.i4.s 4
add
stloc.2
ldc.i4.s 5
ldc.i4.s 7
mul
stloc.1
call void [mscorlib]System.Console::WriteLine(Method not declared: twoparams.outTwoInts)
ret
}
}
