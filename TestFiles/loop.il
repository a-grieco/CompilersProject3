.assembly extern mscorlib {}
.assembly addnums {}
.class public loop
{
.method public static void main() cil managed 
{
.entrypoint
.maxstack 42

.locals init(
   [0] int32 i,
   [1] int32 gobble
)
ldc.i4.s 0
stloc.0
ldc.i4.s 1
stloc.1
ldstr "Beginning loop test"
call void [mscorlib]System.Console::WriteLine(string)
br.s while_cond_loc0
while_true_loc0:
ldloc.1
call void [mscorlib]System.Console::WriteLine(int32)
ldloc.0
ldc.i4.s 1
add
stloc.0
ldloc.1
ldloc.0
mul
stloc.1
while_cond_loc0:
ldloc.0
ldc.i4.s 10
clt
brtrue.s while_true_loc0
ret
}
}
