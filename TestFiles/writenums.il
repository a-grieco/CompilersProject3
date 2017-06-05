.assembly extern mscorlib {}
.assembly addnums {}
.class public writenums
{
.method public static void main() cil managed 
{
.entrypoint
.maxstack 42

ldc.i4.s 5
call void [mscorlib]System.Console::WriteLine(int32)
ldc.i4.s 5
ldc.i4.s 2
mul
ldc.i4.s 6
ldc.i4.s 4
div
add
call void [mscorlib]System.Console::WriteLine(int32)
ret
}
}
