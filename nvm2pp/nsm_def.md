nvm2pp assembly definition

### register opcodes:
LDB r v 		- load byte to register

LDI r v 		- load int to register

LDL r v 		- load long to register


MOVB t f 		- move byte from register f -> t

MOVI t f 		- move int from register f -> t

MOVL t f 		- move long from register f -> t


### stack opcodes
PUSHB v 		- push byte to stack

PUSHI v 		- push int to stack

PUSHL v 		- push long to stack

PUSHBR r 		- push byte register

PUSHIR r 		- push int register

PUSHLR r 		- push long register


POPB r 			- pop byte

POPI r 			- pop int

POPL r 			- pop long


### memory opcodes
READB r a 		- read byte from address a

READI r a 		- read int from address a

READL r a 		- read long from address a

READRB r ra 	- read byte from address of register ra

READRI r ra 	- read int from address of register ra

READRL r ra 	- read long from address of register ra


**simmilar codes for writing**

### Jumping

JMP a 			- jump to address a

CALL a 			- call address a

RET				- return on callstack

JMPR r			- jump to address of register r

CALLR r			- call address of register r


### Math

ADD a b		- add a to b

SUB a b		- subtract a from b

MUL a b		- multiply a to b

DIV a b		- divide a by b

POW a b		- a to the power of b

SQRT a		- sqare root of a


### Memory allocation

STACKALLOC	- allocate an object of size eax to the stack

STACKFREE	- free an object from the stack

MALLOC		- alloc an object of size eax to the heap

FREE		- free an object in the heap at address esi and eax