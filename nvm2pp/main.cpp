#include "vm.hpp"

using namespace std;

int main()
{
	vm v{};
	v.flags(0x7fff);
	for(auto f : v.flagsvec()) {
		cout << f << ",";
	}
	cout << v.flags() << endl;
	return 0;
}