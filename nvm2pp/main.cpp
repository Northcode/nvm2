#include "vm.hpp"
#include "pager.hpp"

using namespace std;

int main()
{
	pageDirectoryEntry pde{0};
	pde.present = true;
	pde.accessed = true;
	pde.readwrite = true;
	pde.address = 2;
	int p = pde;
	for(int i = 0; i < 32; i++)
		cout << ((p >> i) & 0x1) << ",";
	cout << endl;
	pde = pageDirectoryEntry(p);
	cout << pde.present << " " << pde.accessed << " " << pde.address << endl;
	
	
	return 0;
	vm v{};
	v.flags(0x7fff);
	for(auto f : v.flagsvec()) {
		cout << f << ",";
	}
	cout << v.flags() << endl;
	return 0;
}