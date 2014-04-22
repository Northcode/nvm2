LD DA [_prg_end]
SET_PAGE_STACK
LD DA [_prg_end]+1024
SET_PAGE_HEAP
PAGE_INIT_MEM
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
READUI
LODS
MV DA DB
LD A 1
INT 1
RET
_main:
LD DA [_main_a]
LD AX 5
WRITEI
LD DA [s_0]
LODS
CALL [_printf]
RET
s_0: DS "hello world!"
_printf_s: DUI 0
_main_a: DI 0
_prg_end:
