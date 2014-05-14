#include "vm.hpp"
#include "pager.hpp"

using namespace std;

int main()
{
	TLB tlb{};
	tlb.flush();
	pageTable pt{};
	pt.present = true;
	pt.readwrite = true;
	tlb.setPageTable(0,pt);
	pageTable ptl = tlb.getPageTable(0);
	ptl.address = 5;
	tlb.setPageTable(0,ptl);
	cout << tlb.getPageTable(0).address << endl;
	return 0;
}