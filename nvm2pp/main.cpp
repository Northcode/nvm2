#include "vm.hpp"
#include "pager.hpp"

using namespace std;

int main()
{
	pageAddress pa{0};
	pa.ptindex = 5;
	pa.page = 3;
	pa.offset = 10;
	int p = pa;
	for(int i = 0; i < 32; i++)
		cout << ((p >> i) & 0x1) << ",";
	cout << endl;
	pa = pageAddress(p);
	cout << pa.ptindex << " " << pa.page << " " << pa.offset << endl;
	
	
	return 0;
	vm v{};
	v.flags(0x7fff);
	for(auto f : v.flagsvec()) {
		cout << f << ",";
	}
	cout << v.flags() << endl;
	return 0;
}