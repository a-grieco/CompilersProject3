.assembly extern mscorlib {}
.assembly addnums {}
.class public fact2
{
.method public static int32 factorial(int32) 
{
.maxstack 42
ldarg.0
ldc.i4 0
ceq
brfalse.s if_false_loc0
ldc.i4 1
br.s if_end_loc0
if_false_loc0:
ldarg.1
ldarg.2
ldc.i4 1
sub
call int32 fact2::factorial(int32)
mul
if_end_loc0:
ret
}
.method public static void main() 
{
.entrypoint
.maxstack 42

.locals init(
   [0] int32 i,
   [1] int32 gobble
)
ldc.i4 0
stloc.0
ldstr "TCCL recursive factorial test"
call void [mscorlib]System.Console::WriteLine(string)
br.s while_cond_loc0
while_true_loc0:
ldloc.0
call int32 fact2::factorial(i)
stloc.1
ldloc.1
call void [mscorlib]System.Console::WriteLine(int32)
ldloc.0
ldc.i4 1
add
stloc.0
while_cond_loc0:
ldloc.0
ldc.i4 15
clt
brtrue.s while_true_loc0
ret
}
}
