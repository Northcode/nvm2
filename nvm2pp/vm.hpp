#pragma once
#include <iostream>
#include "ram.hpp"

constexpr int CALLSTACK_SIZE{512};

typedef unsigned char byte ;

int ltoi(long int l) { return l & (0xffff); }
short int ltos(long int l) { return l & (0xff); }
byte ltob (long int l) { return l & 0xf; }

class vm
{
public:

	ram memory;

	// Registers
	long int rax;
	long int rbx;
	long int rcx;
	long int rdx;
	
	long int rbp;
	long int rsp;
	long int rip;
	long int rsi;
	long int rdi;
	
	long int rflags;
	long int cr0;
	long int cr1;
	long int cr2;
	int cr3;
	
	//Sub Registers	
	int eax() {	return ltoi(rax); }
	int ebx() { return ltoi(rbx); }
	int ecx() { return ltoi(rcx); }
	int edx() { return ltoi(rdx); }
	
	void eax(int value) { rax = static_cast<long int>(value); }
	void ebx(int value) { rbx = static_cast<long int>(value); }
	void ecx(int value) { rcx = static_cast<long int>(value); }
	void edx(int value) { rdx = static_cast<long int>(value); }
	
	short int ax() { return ltos(rax); }
	short int bx() { return ltos(rbx); }
	short int cx() { return ltos(rcx); }
	short int dx() { return ltos(rdx); }
	
	void ax(short int value) { rax = static_cast<long int>(value); }
	void bx(short int value) { rbx = static_cast<long int>(value); }
	void cx(short int value) { rcx = static_cast<long int>(value); }
	void dx(short int value) { rdx = static_cast<long int>(value); }
	
	byte al() { return ltob(rax); }
	byte bl() { return ltob(rbx); }
	byte cl() { return ltob(rcx); }
	byte dl() { return ltob(rdx); }
	
	void al(byte value) { rax = static_cast<long int>(value); }
	void bl(byte value) { rbx = static_cast<long int>(value); }
	void cl(byte value) { rcx = static_cast<long int>(value); }
	void dl(byte value) { rdx = static_cast<long int>(value); }
	
	int ebp() { return ltoi(rbp); }
	int esp() { return ltoi(rsp); }
	int eip() { return ltoi(rip); }
	int esi() { return ltoi(rsi); }
	int edi() { return ltoi(rdi); }
	
	void edp(int value) { rbp = static_cast<long int>(value); }
	void esp(int value) { rsp = static_cast<long int>(value); }
	void eip(int value) { rip = static_cast<long int>(value); }
	void esi(int value) { rsi = static_cast<long int>(value); }
	void edi(int value) { rdi = static_cast<long int>(value); }
	
	int flags() { return ltoi(rflags); }
	void flags(int value) { rflags = static_cast<long int>(value); }
};