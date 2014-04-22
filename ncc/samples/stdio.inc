void printf(string s) {
	asm "LD DA [_printf_s]";
	asm "READUI";
	asm "LODS";
	asm "MV DA DB";
	asm "LD A 1";
	asm "INT 1";
}
