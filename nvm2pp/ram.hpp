#pragma once
#include <iostream>
#include <vector>
#include <algorithm>
#include <memory>

constexpr int FRAME_SIZE{4*1024};

std::vector<unsigned char> ToBytes(int paramInt)
{
    std::vector<unsigned char> arrayOfByte{4};
    for (int i = 0; i < 4; i++)
        arrayOfByte[3 - i] = (paramInt >> (i * 8));
    return arrayOfByte;
}

struct frame
{
	int address;
	bool isFree;
	int pageTable;

	frame()
	{
		isFree = true;
		pageTable = 0;
	}
};

class ram
{
private:
	std::vector<unsigned char> data;
	std::vector<frame> frames;
public:
	ram(int size) {
		data = std::vector<unsigned char>(size);
		frames = std::vector<frame>(size/FRAME_SIZE);
    fillFrames();
	}

	frame getFrame(int index) {
		return frames[index];
	}

  void fillFrames() {
    for(int i = 0; i < frames.size(); i++) {
      frames[i].address = i * 4096;
    }
  }

	std::vector<frame>::iterator findFreeFrame() {
		return find_if(begin(this->frames),end(this->frames),[&](const frame& f) { return f.isFree; });
	}

  void setFrameState(int index, bool state) {
    frames[index].isFree = state;
  }

	//write functions
	void write(int address, unsigned char value) {
		data[address] = value;
	}

	void write(int address, int value) {
		std::vector<unsigned char> bval = ToBytes(value);
		for(int i = 0; i < 4; i++)
			write(address + i, bval[i]);
	}

	unsigned char read(int address) {
		return data[address];
	}

	int readInt(int address) {
		int i = (data[address + 3] << 24) | (data[address + 2] << 16) | (data[address + 1] << 8) | (data[address + 0]);
		return i;
	}
};
