#include "vm.hpp"
#include "pager.hpp"

using namespace std;

int main()
{
	ram r{4*4*1024};
	auto f = r.findFreeFrame();
	f->isFree = false;
	cout << f->address << endl;
	auto f2 = r.findFreeFrame();
	cout << f2->address << endl;
	return 0;
}
