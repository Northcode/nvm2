#include "ram.hpp"
#include <memory>
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

struct pageTableEntry
{
	bool present;
	bool readwrite;
	bool mode;
	bool writethrough;
	bool chachedisabled;
	bool accessed;
	bool dirty;
	bool global;
	int address;

	pageTableEntry(int value) {
		present 		= getBit(value,0);
		readwrite 		= getBit(value,1);
		mode 			= getBit(value,2);
		writethrough 	= getBit(value,3);
		chachedisabled 	= getBit(value,4);
		accessed 		= getBit(value,5);
		dirty 			= getBit(value,7);
		global			= getBit(value,8);
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
		v |= (dirty << 7);
		v |= (global << 8);
		v |= (address << 11);
		return v;
	}
};

struct pageAddress
{
	int ptindex;
	int page;
	int offset;

	pageAddress(int address) {
		ptindex = address & 0x3ff;
		page = (address & 0xffc00) >> 10;
		offset = address >> 20;
	}

	operator int() {
		int v = 0;
		v |= ptindex;
		v |= (page << 10);
		v |= (offset << 20);
		return v;
	}
};

class TLB
{
	std::vector<pageDirectoryEntry> pageDirectoryCache;
	std::vector<pageTableEntry> pageTableCache;
public:

	TLB() {
		pageDirectoryCache = vector<pageDirectoryEntry>(1024);
	}

	void flush() {
		pageTableCache = vector<pageTableEntry>(1024);
	}

	void updatePTCache(int address,std::shared_ptr<ram> memory) {
		for(int i = 0; i < 1024; i++) {
			int entry{memory->readInt(address + i * 4)};
			pageTableEntry pte{entry};
			pageTableCache[i] = pte;
		}
	}

	std::shared_ptr<pageDirectoryEntry> getPT(int index) {
		return std::shared_ptr<pageDirectoryEntry>(pageDirectoryEntry[index]);
	}

	pageTableEntry getPage(int index) {
		return std::shared_ptr<pageDirectoryEntry>(pageTableEntry[index]);
	}
};

class pager
{

};
