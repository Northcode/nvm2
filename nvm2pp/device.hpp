#pragma once
#include "types.hpp"


class vm_device
{
	virtual void out(byte data) = 0;
	virtual void out(int data) = 0;

	virtual byte inb() = 0;
	virtual int ini() = 0;
};
