.assembly extern mscorlib {}
.assembly addnums {}
.class public logictest
{
.method public static void outStrln(string) 
{
.maxstack 42
call void [mscorlib]System.Console::WriteLine(No method signature found matching: (Project4.PrimitiveTypeStringDescriptor))
ret
}
.method public static void main() 
{
.entrypoint
.maxstack 42

.locals init(
   [0] int32 i,
   [1] int32 j
)
ldc.i4 0
stloc.0
ldc.i4 1
stloc.1
call void [mscorlib]System.Console::WriteLine(No method signature found matching: (Project4.LiteralTypeDescriptor))
ldloc.0
ldc.i4 10
clt
ldloc.1
ldc.i4 1
ceq
and
brfalse.s if_false_loc0
call void [mscorlib]System.Console::WriteLine(No method signature found matching: (Project4.LiteralTypeDescriptor))
br.s if_end_loc0
if_false_loc0:
ldloc.1
stloc.0
if_end_loc0:
ldloc.0
ldc.i4 10
clt
ldloc.1
ldc.i4 0
ceq
and
brfalse.s if_false_loc1
call void [mscorlib]System.Console::WriteLine(Method not declared: logictest.outStrln)
br.s if_end_loc1
if_false_loc1:
call void [mscorlib]System.Console::WriteLine(Method not declared: logictest.outStrln)
if_end_loc1:
ldloc.0
ldc.i4 10
ceq
ldloc.1
ldloc.0
ceq
something bad happened
brfalse.s if_false_loc2
ldloc.0
stloc.1
br.s if_end_loc2
if_false_loc2:
call void [mscorlib]System.Console::WriteLine(Method not declared: logictest.outStrln)
if_end_loc2:
ldloc.0
ldc.i4 1
ceq
ldloc.1
ldc.i4 1
ceq
something bad happened
brfalse.s if_false_loc3
call void [mscorlib]System.Console::WriteLine(Method not declared: logictest.outStrln)
br.s if_end_loc3
if_false_loc3:
call void [mscorlib]System.Console::WriteLine(Method not declared: logictest.outStrln)
if_end_loc3:
ret
}
}
