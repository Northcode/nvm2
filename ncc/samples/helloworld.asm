CALL [_main]
PAGE_RET
_printf:
POPI AX
PUSHR AX
LD BX 4
ADD INT
MV AX EX
MALLOC
MV DB DA
LD DA [_printf_s]
WRITEUI
LD DA [_printf_s]
LODS
LD A 1
INT 1
RET
_main:
LD DA [_main_a]
LD AX 5
WRITEI
LD DA [s_0]
LODS
RET
s_0: DS "hello world!"
_printf_s: DUI 0
_main_a: DI 0

