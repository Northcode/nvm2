#include "ram.hpp"
#pragma once

bool getBit(int value, int bit) {
	return value & (1 << bit);
}

struct pageDirectoryEntry
{
	bool present;
	bool readwrite;
	bool mode;
	bool writethrough;
	bool chachedisabled;
	bool accessed;
	bool size;
	int address;
	
	pageDirectoryEntry(int value) {
		present 		= getBit(value,0);
		readwrite 		= getBit(value,1);
		mode 			= getBit(value,2);
		writethrough 	= getBit(value,3);
		chachedisabled 	= getBit(value,4);
		accessed 		= getBit(value,5);
		size 			= getBit(value,7);
		address			= value >> 11;
	}
	
	operator int() {
		int v = 0;
		v |= (present << 0);
		v |= (readwrite << 1);
		v |= (mode << 2);
		v |= (writethrough << 3);
		v |= (chachedisabled << 4);
		v |= (accessed << 5);
		v |= (size << 7);
		v |= (address << 11);
		return v;
	}
};

class pager
{
	
};
