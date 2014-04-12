void printf(string s) {
	asm "LD A 1";
	asm "INT 1";
	return;
}
