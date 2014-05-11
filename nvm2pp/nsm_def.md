nvm2pp assembly definition

### register opcodes:
LDB r v 		- load byte to register

LDI r v 		- load int to register

LDL r v 		- load long to register


MOV t f     - copy value from register f to register t

MOV t [a]  - copy value from address a to register t

MOV t [r+o] - copy value from address of register r with offest o

MOV [a] t   - copy value from register t to address a
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

-- For all jumping, if t=1, a is a register

JMP t a 		- jump to address a

CALL t a 		- call address a

RET				- return on callstack

JEQ t a			- jump if equal

JNE t a			- jump if not equal

JLT t a			- jump if comparator is less than

JLE t a			- jump if comparator is less than or equal

JGE t a			- jump if comparator is greater than

JGT t a			- jump if comparator is greater than or equal

### Math

-- for all math, t = 0,1,2,3 for byte,short,int and long operations

ADD t a b		- add a to b

SUB t a b		- subtract a from b

MUL t a b		- multiply a to b

DIV t a b		- divide a by b

POW t a b		- a to the power of b

SQRT t a		- sqare root of a

## Binary math

OR t r r		- binary OR on two registers

AND t r r		- binary AND on two registers

XOR t r r		- binary XOR on two registers

NOT t r			- binary NOT on a register

### Memory allocation

STACKALLOC	- allocate an object of size eax to the stack

STACKFREE	- free an object from the stack

MALLOC		- alloc an object of size eax to the heap

FREE		- free an object in the heap at address esi and eax

### Interupts

INT	i		- fire interupt i

RSI i		- register software interupt i

SWI i		- software interupt i

### Comparison

CMPB r,r	- compare bytes

CMPI r,r	- compare ints

CMPL r,r	- compare longs
