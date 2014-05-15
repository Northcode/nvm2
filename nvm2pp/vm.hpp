#pragma once
#include <iostream>
#include <memory>
#include "ram.hpp"
#include "pager.hpp"
#include "types.hpp"

constexpr int CALLSTACK_SIZE{512};


int ltoi(long l) { return l & (0xffff); }
short ltos(long l) { return l & (0xff); }
byte ltob (long l) { return l & 0xf; }

class vm
{
public:

	//Helper classes and ram
	std::unique_ptr<ram> memory;
	TLB tlb;

	vm()
	{
		memory = std::unique_ptr<ram>(new ram(4*1024));
		tlb = TLB();
	}

	// Registers
	long rax;
	long rbx;
	long rcx;
	long rdx;

	long rbp;
	long rsp;
	long rip;
	long rsi;
	long rdi;

	int cs;
	int ds;
	int ss;
	int es;

	bool CARRY;
	bool PARITY;
	bool ADJUST;
	bool ZERO;
	bool SIGNED;
	bool TRAP;
	bool INTERUPT_ENABLE;
	bool DIRECTION;
	bool OVERFLOW_F;
	bool PRIVILEGE0;
	bool PRIVILEGE1;
	bool NESTED;

	bool F0;
	bool F1;
	bool F2;
	bool F3;

	long cr0;
	long cr1;
	long cr2;
	int cr3;


	//Sub Registers
	int eax() { return ltoi(rax); }
	int ebx() { return ltoi(rbx); }
	int ecx() { return ltoi(rcx); }
	int edx() { return ltoi(rdx); }

	void eax(int value) { rax = static_cast<long>(value); }
	void ebx(int value) { rbx = static_cast<long>(value); }
	void ecx(int value) { rcx = static_cast<long>(value); }
	void edx(int value) { rdx = static_cast<long>(value); }

	short ax() { return ltos(rax); }
	short bx() { return ltos(rbx); }
	short cx() { return ltos(rcx); }
	short dx() { return ltos(rdx); }

	void ax(short value) { rax = static_cast<long>(value); }
	void bx(short value) { rbx = static_cast<long>(value); }
	void cx(short value) { rcx = static_cast<long>(value); }
	void dx(short value) { rdx = static_cast<long>(value); }

	byte al() { return ltob(rax); }
	byte bl() { return ltob(rbx); }
	byte cl() { return ltob(rcx); }
	byte dl() { return ltob(rdx); }

	void al(byte value) { rax = static_cast<long>(value); }
	void bl(byte value) { rbx = static_cast<long>(value); }
	void cl(byte value) { rcx = static_cast<long>(value); }
	void dl(byte value) { rdx = static_cast<long>(value); }

	int ebp() { return ltoi(rbp); }
	int esp() { return ltoi(rsp); }
	int eip() { return ltoi(rip); }
	int esi() { return ltoi(rsi); }
	int edi() { return ltoi(rdi); }

	void edp(int value) { rbp = static_cast<long>(value); }
	void esp(int value) { rsp = static_cast<long>(value); }
	void eip(int value) { rip = static_cast<long>(value); }
	void esi(int value) { rsi = static_cast<long>(value); }
	void edi(int value) { rdi = static_cast<long>(value); }

	std::vector<bool> flagsvec() {
		return { CARRY, F0, PARITY, F1, ADJUST, F2, ZERO, SIGNED, TRAP, INTERUPT_ENABLE, DIRECTION, OVERFLOW_F, PRIVILEGE0, PRIVILEGE1, NESTED, F3 };
	}

	short flags() {
		std::vector<bool> flags{flagsvec()};
		short sflags = 0;
	    for (int i = 0; i < 15; ++i) {
	        if (flags[i]) {
	            sflags |= 1 << i;
	        }
	    }
	    return sflags;
	}

	void flags(short value) {
		std::vector<bool> nflags = std::vector<bool>(16);
		int count = 0;
    	while(value) {
	        if (value&1)
	            nflags[count] = true;
	        else
	            nflags[count] = false;
	        value>>=1;
	        count++;
	    }

			CARRY 			= 	nflags[0];
	    F0					=	nflags[1];
	    PARITY			= 	nflags[2];
	    F1					=	nflags[3];
	    ADJUST			= 	nflags[4];
	    F2					=	nflags[5];
	    ZERO				=	nflags[6];
	    SIGNED			=	nflags[7];
	    TRAP				=	nflags[8];
	    INTERUPT_ENABLE	=	nflags[9];
	    DIRECTION		 =	nflags[10];
	    OVERFLOW_F		=	nflags[11];
	    PRIVILEGE0		=	nflags[12];
	    PRIVILEGE1		=	nflags[13];
	    NESTED				=	nflags[14];
	    F3						=	nflags[15];
	}

	void enable_paging() {
		cr0 |= (true << 31);
	}

	void disable_paging() {
		cr0 |= (false << 31);
	}

	void alloc_pagetable(int index) {
		bool paging = cr0 & (1 << 31);
		if(paging) {
			//Find free frame
			auto freeFrame = memory->findFreeFrame();
			//Create page table
			pageTable pt{0};
			freeFrame->isFree = false;
			pt.address = freeFrame->address;
			pt.present = true;
			pt.readwrite = true;
			pt.cachedisabled = true;
			pt.accessed = false;
			pt.size = false;
			//Store page table in page directory
			write_page_table(cr3 + index * 4, pt);
		}
	}

	void write_page_table(int address, pageTable pt) {
		memory->write(address,(int)pt);
	}

	void alloc_page(int pt, bool mode) {
		bool paging = cr0 & (1 << 31);
		if(paging) {
			//Find free frame
			auto freeFrame = memory->findFreeFrame();
			//Create page
			page pg{0};
			freeFrame->isFree = false;
			pg.address = freeFrame->address;
			pg.present = true;
			pg.readwrite = true;
			pg.mode = mode;
			pg.cachedisabled = true;
			//write page to page table
			write_page(pt,pg);
		}
	}

	void write_page(int pt, page pg) {
		//read page table address
		int pti = memory->readInt(cr3 + pt*4);
		pageTable ptc{pti};
		int addr = ptc.address;
		//Look for free page slot
		for(int i = 0; i < 1024; i++) {
			int pgi = memory->readInt(addr + i * 4);
			page pgc{pgi};
			if(!pgc.present) {
				memory->write(addr + i * 4, (int)pg);
				return;
			}
		}
	}

	void free_page(int pt, int pg) {
		//get address of page table
		int pti = memory->readInt(cr3 + pt*4);
		pageTable ptc{pti};
		int addr = ptc.address;

		//clear present bit in page
		memory->write(addr + pg * 4, false);
	}

	void free_pagetable(int pt) {
		memory->write(cr3 + pt * 4, false);
	}
};
