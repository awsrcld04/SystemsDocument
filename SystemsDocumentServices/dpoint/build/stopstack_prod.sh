cd /
cd /apps/mongrel2-v1.11.0
m2sh stop -name prod
python stopper.py OnUpload.py.pid
python stopper.py QueueForProcessingManager.py.pid
python stopper.py ProcessData_Q1.py.pid
python stopper.py ProcessData_Q2.py.pid
python stopper.py ProcessData_Q3.py.pid
python stopper.py ProcessData_Q4.py.pid
python stopper.py PickupReq.py.pid
python stopper.py SystemStatus_prod.py.pid
rm *.pyc
rm *.py.pid
