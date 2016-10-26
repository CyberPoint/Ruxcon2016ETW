using ETW_IE_InfoLeak_Demo_Parser;
using Microsoft.Diagnostics.Tracing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ETW_IE_InfoLeak_Demo_GUI
{
    public partial class Form1 : Form
    {
        delegate void SetTextCallback(Control view, string text);
        delegate void AddNodeCallback(TreeNode parent, TreeNode newNode);
        delegate void AddNodeCallback2(TreeView parent, TreeNode newNode);
        delegate void ControlCallback(Control c);
        delegate void TreeNodeCallback(TreeNode tn);
        Dictionary<string, TreeNode> eventNameNodes = new Dictionary<string, TreeNode>();
        TreeNode CookiesStoredNode;
        TreeNode URLsAccessedNode;
        TreeNode RequestsMadeNode;
        IE_Demo_Parser parser = new IE_Demo_Parser();
        private bool callbacksRegistered;

        public Form1()
        {
            InitializeComponent();
            CookiesStoredNode = extractedDataTreeView.Nodes.Find("CookiesStoredNode", false)[0];
            URLsAccessedNode = extractedDataTreeView.Nodes.Find("URLsAccessedNode", false)[0];
            RequestsMadeNode = extractedDataTreeView.Nodes.Find("RequestsMadeNode", false)[0];

        }


        private void btnStartProvider_Click(object sender, EventArgs e)
        {
            btnStartProvider.Enabled = false;
            parser.CreateSession();
            if (!callbacksRegistered)
            {
                registerCallbacks();
            }
            parser.EnableProvider(TraceEventLevel.Verbose);
            Console.WriteLine("Provider started");
            providerStatusLabel.Text = "Started";
            btnStopProvider.Enabled = true;
            btnStartConsumer.Enabled = true;
        }

        private void registerCallbacks()
        {
            parser.EventCallback += delegate (IE_Demo_Parser.EventData data)
            {
                TreeNode eventNameNode;
                if (!eventNameNodes.TryGetValue(data.EventName, out eventNameNode))
                {
                    eventNameNode = new TreeNode(data.EventName);
                    AddNode2(treeView1, eventNameNode);
                    eventNameNodes.Add(data.EventName, eventNameNode);
                }

                TreeNode eventNode = new TreeNode(data.FormattedMessage);
                foreach (KeyValuePair<string, string> kvp in data.Properties)
                {
                    eventNode.Nodes.Add(kvp.Key + ": " + kvp.Value);
                }
                AddNode(eventNameNode, eventNode);
                //Console.WriteLine(data.InfoString + "\r\n");
                AppendText(textBox1, data.InfoString + "\r\n");

                SetText(numParsedEventsLabel, parser.NumParsedEvents.ToString());
                if (parser.NumParsedEvents % 100 == 0)
                {
                    SetText(numDroppedEventsLabel, parser.EventsLost.ToString());
                }
            };

            parser.EventMissedCallback += delegate (int numMissedEvents)
            {
                SetText(requestsMissingLabel, numMissedEvents.ToString());
                SetText(numDroppedEventsLabel, parser.EventsLost.ToString());
            };

            parser.ExtractedDataCallback += delegate (JObject data, int type)
            {
                TreeNode scrollToNode = null;
                BeginUpdate(extractedDataTreeView);
                switch (type) {
                    case IE_Demo_Parser.COOKIE_STORED:
                        string domain = data["data"]["Domain"].ToString();
                        TreeNode[] nodesForDomain = CookiesStoredNode.Nodes.Find(domain, false);
                        TreeNode cookieNode = new TreeNode(data["label"].ToString());
                        foreach (JProperty p in data["data"])
                        {
                            cookieNode.Nodes.Add(p.Name + ": " + p.Value);
                        }
                        if (nodesForDomain.Length > 0)
                        {
                            AddNode(nodesForDomain[0], cookieNode);
                        }
                        else
                        {
                            TreeNode domainNode = new TreeNode(domain);
                            domainNode.Name = domain;
                            domainNode.Nodes.Add(cookieNode);
                            AddNode(CookiesStoredNode, domainNode);
                        }
                        break;
                    case IE_Demo_Parser.REQUEST_MADE:
                        TreeNode requestNode = new TreeNode(data["label"].ToString());
                        foreach (JProperty p in data["data"])
                        {
                            if (p.Name == "POST Parameters")
                            {
                                foreach (JToken postParamsObj in (JArray)p.Value)
                                {
                                    TreeNode postParamsNode = new TreeNode(p.Name);
                                    postParamsNode.ToolTipText = postParamsObj["raw"].ToString();
                                    requestNode.Nodes.Add(postParamsNode);
                                    foreach (JToken postParam in (JArray)postParamsObj["parsedArray"])
                                    {
                                        if (postParam.HasValues)
                                        {
                                            postParamsNode.Nodes.Add(((JProperty)postParam.First).Name + "=" + ((JProperty)postParam.First).Value.ToString());
                                        }
                                        else
                                        {
                                            postParamsNode.Nodes.Add(postParam.ToString());
                                        }

                                    }
                                }
                            }
                            else if (p.Name == "POST Parameters (raw)")
                            {
                                foreach (JToken postParamsObj in (JArray)p.Value)
                                {
                                    TreeNode postParamsNode = new TreeNode(p.Name);
                                    postParamsNode.ToolTipText = postParamsObj.ToString();
                                    requestNode.Nodes.Add(postParamsNode);
                                    postParamsNode.Nodes.Add(postParamsObj.ToString());
                                }
                            }
                            else if (p.Name == "Other Headers")
                            {
                                foreach (JToken otherHeadersObj in (JArray)p.Value)
                                {
                                    TreeNode otherHeadersNode = new TreeNode(p.Name);
                                    otherHeadersNode.ToolTipText = otherHeadersObj.ToString();
                                    requestNode.Nodes.Add(otherHeadersNode);
                                    otherHeadersNode.Nodes.Add(otherHeadersObj.ToString());
                                }
                            }
                            else if (p.Name == "Headers" || p.Name == "Response Headers")
                            {
                                TreeNode headerParamsNode = new TreeNode(p.Name);
                                headerParamsNode.ToolTipText = p.Value["raw"].ToString();
                                requestNode.Nodes.Add(headerParamsNode);
                                foreach (JToken headerParam in p.Value["parsedArray"])
                                {
                                    if(headerParam.HasValues)
                                    {
                                        headerParamsNode.Nodes.Add(((JProperty)headerParam.First).Name + ":" + ((JProperty)headerParam.First).Value.ToString());
                                    }
                                    else
                                    {
                                        headerParamsNode.Nodes.Add(headerParam.ToString());
                                    }
                                }
                            }
                            else
                            {
                                requestNode.Nodes.Add(p.Name + ": " + p.Value);
                            }
                        }
                        AddNode(RequestsMadeNode, requestNode);
                        if (checkBox_autoScrollRequests.Checked)
                        {
                            scrollToNode = requestNode;
                        }
                        break;
                    case IE_Demo_Parser.URL_ACCESSED:
                        string url = data["data"]["URL"].ToString();
                        TreeNode[] nodesForURL = URLsAccessedNode.Nodes.Find(url, false);
                        if (nodesForURL.Length > 0)
                        {
                            AddNode(nodesForURL[0], new TreeNode(data["data"]["Timestamp"].ToString()));
                        }
                        else
                        {
                            TreeNode urlNode = new TreeNode(url);
                            urlNode.Name = url;
                            urlNode.Nodes.Add(data["data"]["Timestamp"].ToString());
                            AddNode(URLsAccessedNode, urlNode);
                        }

                        break;
                }
                EndUpdate(extractedDataTreeView);
                if (scrollToNode != null)
                {
                    EnsureVisible(scrollToNode);
                    scrollToNode = null;
                }
            };

            callbacksRegistered = true;
        }

        private TreeNode CopyChildNodes(TreeNode searchNode, TreeNode newParentNode = null)
        {
            if (newParentNode == null)
            {
                newParentNode = new TreeNode(searchNode.Text);
                newParentNode.Name = searchNode.Name;
            }
            foreach (TreeNode tn1 in searchNode.Nodes)
            {
                TreeNode tmpNode = new TreeNode(tn1.Text);
                tmpNode.Name = tn1.Name;
                newParentNode.Nodes.Add(tmpNode);
                CopyChildNodes(tn1, tmpNode);
            }
            return newParentNode;
        }
        private void btnStopProvider_Click(object sender, EventArgs e)
        {
            btnStopProvider.Enabled = false;
            btnStartConsumer.Enabled = false;
            parser.Stop();
            Console.WriteLine("Stopped");
            providerStatusLabel.Text = "Stopped";
            btnStartProvider.Enabled = true;
        }

        private void btnStartConsumer_Click(object btnSender, EventArgs ntnEventArgs)
        {
            btnStartConsumer.Enabled = false;
            Thread oThread = new Thread(new ThreadStart(StartConsuming));
            oThread.Start();
            Console.WriteLine("Started consuming events");
        }

        private void StartConsuming()
        {
            SetText(consumerStatusLabel, "Started");
            parser.Process();
            SetText(consumerStatusLabel,  "Stopped");
        }
        private void SetText(Control view, string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (view.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] {view, text });
            }
            else
            {
                view.Text = text;
            }
        }

        private void AppendText(Control view, string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (view.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(AppendText);
                this.Invoke(d, new object[] { view, text });
            }
            else
            {
                ((TextBox)view).AppendText(text);
            }
        }
        private void AddNode(TreeNode parent, TreeNode newNode)
        {
            if (parent.TreeView.InvokeRequired)
            {
                AddNodeCallback d = new AddNodeCallback(AddNode);
                this.Invoke(d, new object[] { parent, newNode });
            }
            else
            {
                parent.Nodes.Add(newNode);
            }
        }
        private void AddNode2(TreeView parent, TreeNode newNode)
        {
            if (parent.InvokeRequired)
            {
                AddNodeCallback2 d = new AddNodeCallback2(AddNode2);
                this.Invoke(d, new object[] { parent, newNode });
            }
            else
            {
                parent.Nodes.Add(newNode);
            }
        }
        private void BeginUpdate(Control tv)
        {
            if (tv.InvokeRequired)
            {
                ControlCallback d = new ControlCallback(BeginUpdate);
                this.Invoke(d, new object[] { tv });
            }
            else
            {
                ((TreeView)tv).BeginUpdate();
            }
        }

        private void EndUpdate(Control tv)
        {
            if (tv.InvokeRequired)
            {
                ControlCallback d = new ControlCallback(EndUpdate);
                this.Invoke(d, new object[] { tv });
            }
            else
            {
                ((TreeView)tv).EndUpdate();
            }
        }

        private void EnsureVisible(TreeNode tn)
        {
            if (tn.TreeView.InvokeRequired)
            {
                TreeNodeCallback d = new TreeNodeCallback(EnsureVisible);
                this.Invoke(d, new object[] { tn });
            }
            else
            {
                tn.EnsureVisible();
            }
        }

        private void treeView_CopySelectedNodeText(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode clickedNode = ((TreeView)sender).SelectedNode;
                if (clickedNode != null)
                {
                    if (clickedNode.ToolTipText.Length > 0)
                    {
                        Clipboard.SetText(clickedNode.ToolTipText);
                        Console.WriteLine(clickedNode.ToolTipText);

                    }
                    else
                    {
                        Clipboard.SetText(clickedNode.Text);
                        Console.WriteLine(clickedNode.Text);
                    }
                }
                else
                {
                    Console.WriteLine("Could not copy text, no node selected");

                }
            }
        }

        private void btnOpenEtlFile_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                parser.CreateSource(openFileDialog1.FileName);
                registerCallbacks();

                Thread oThread = new Thread(new ThreadStart(StartConsuming));
                oThread.Start();

            }

        }

        /* //TODO: finish when TraceEvent supports consuming events in realtime and saving to a file
        private void btnStartAndSave_Click(object sender, EventArgs e)
        {
            
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string file = saveFileDialog1.FileName;
                btnStartProvider.Enabled = false;
                //btnStartAndSave.Enabled = false;
                createSession(file);
                session.EnableProvider(WinINetProviderId, TraceEventLevel.Verbose);//, 0x0000020000000048L);
                Console.WriteLine("Provider started");
                providerStatusLabel.Text = "Started";
                btnStopProvider.Enabled = true;
                btnStartConsumer.Enabled = true;
            }

        }*/


    }
}
