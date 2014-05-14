#include "ram.hpp"
#include <memory>
#pragma once

bool getBit(int value, int bit) {
	return value & (1 << bit);
}

struct pageTable
{
	bool present;
	bool readwrite;
	bool mode;
	bool writethrough;
	bool chachedisabled;
	bool accessed;
	bool size;
	int address;

	pageTable() : pageTable(0) {}

	pageTable(int value) {
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

struct page
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

	page() : page(0) {}

	page(int value) {
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
	std::vector<pageTable> pageDirectoryCache;
	std::vector<page> pageTableCache;
public:

	TLB() {
		this->pageDirectoryCache = std::vector<pageTable>(1024);
	}

	void flush() {
		this->pageTableCache = std::vector<page>(1024);
	}

	void updatePTCache(int address,std::shared_ptr<ram> memory) {
		flush();
		for(int i = 0; i < 1024; i++) {
			int entry{memory->readInt(address + i * 4)};
			pageTableCache[i] = page(entry);
		}
	}

	pageTable getPageTable(int index) {
		return pageDirectoryCache[index];
	}

	void setPageTable(int index, pageTable entry) {
		pageDirectoryCache[index] = entry;
	}

	page getPage(int index) {
		return pageTableCache[index];
	}

	void setPage(int index, page entry) {
		pageTableCache[index] = entry;
	}
};

class pager
{

};
