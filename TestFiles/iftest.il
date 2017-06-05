.assembly extern mscorlib {}
.assembly addnums {}
.class public iftest
{
.method public static void main() cil managed 
{
.entrypoint
.maxstack 42

.locals init(
   [0] int32 i,
   [1] int32 j
)
ldc.i4.s 0
stloc.0
ldc.i4.s 1
stloc.1
ldstr "TCCL if test"
call void [mscorlib]System.Console::WriteLine(string)
ldloc.0
ldc.i4.s 10
clt
brfalse.s if_false_loc0
ldstr "In then part"
call void [mscorlib]System.Console::WriteLine(string)
br.s if_end_loc0
if_false_loc0:
ldloc.1
stloc.0
if_end_loc0:
ldloc.0
ldc.i4.s 10
ceq
brfalse.s if_false_loc1
ldloc.0
stloc.1
br.s if_end_loc1
if_false_loc1:
ldstr "In else part"
call void [mscorlib]System.Console::WriteLine(string)
if_end_loc1:
ret
}
}
