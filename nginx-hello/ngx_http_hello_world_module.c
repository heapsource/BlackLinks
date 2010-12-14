#include <ngx_config.h>    
#include <ngx_core.h>
#include <ngx_http.h> 
  
#include <glib.h> 
#include <mono/jit/jit.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/threads.h>
  
typedef struct {
    ngx_str_t blacklinks_app_dir; 

} ngx_http_blacklinks_loc_conf_t;

ngx_http_blacklinks_loc_conf_t * global_blacklinks_conf;

extern void
mono_security_enable_core_clr ();

extern void
mono_security_set_core_clr_platform_callback (MonoCoreClrPlatformCB callback);

void ensure_mono(ngx_log_t  *log);
MonoString* GetConfigurationAppPathDirectory();
static char *ngx_http_blacklinks_application_configuration(ngx_conf_t *cf, ngx_command_t *cmd, void *conf);
void *
ngx_blacklinks_create_loc_conf(ngx_conf_t *cf);

static ngx_command_t  ngx_http_hello_world_commands[] = {

  { ngx_string("blacklinks_application"),
    NGX_HTTP_LOC_CONF|NGX_CONF_TAKE1,
    ngx_http_blacklinks_application_configuration,
    NGX_HTTP_LOC_CONF_OFFSET,
    0,
    NULL },
    ngx_null_command
}; 


typedef struct
{
	MonoString * Key;
	MonoString * Value;
}  NginxMonoHeader;

typedef struct
{
	MonoString * method_name;
	MonoString * http_protocol;
	MonoString * uri;
	MonoString * args;
	int headers_count;
}  NginxMonoRequestInfo;

void SecureEnvironment();
void extract_mono_headers(ngx_http_request_t *r,NginxMonoRequestInfo * info);



//static u_char  ngx_hello_world[] = "<h1>Hello there T1</h1>";

static ngx_http_module_t  ngx_http_hello_world_module_ctx = {
  NULL,                          /* preconfiguration */
  NULL,                          /* postconfiguration */

  NULL,                          /* create main configuration */
  NULL,                          /* init main configuration */

  NULL,                          /* create server configuration */
  NULL,                          /* merge server configuration */

  ngx_blacklinks_create_loc_conf,                          /* create location configuration */
  NULL                           /* merge location configuration */
};

ngx_module_t ngx_http_hello_world_module = {
  NGX_MODULE_V1,
  &ngx_http_hello_world_module_ctx, /* module context */
  ngx_http_hello_world_commands,   /* module directives */
  NGX_HTTP_MODULE,               /* module type */
  NULL,                          /* init master */
  NULL,                          /* init module */
  NULL,                          /* init process */
  NULL,                          /* init thread */
  NULL,                          /* exit thread */
  NULL,                          /* exit process */
  NULL,                          /* exit master */
  NGX_MODULE_V1_PADDING
};
	MonoImage * assembly_image;
	MonoDomain *domain;
 	MonoClass * myclass;
	MonoAssembly *assembly;
	MonoObject *my_class_instance;
MonoMethodDesc* process_method_desc;
MonoMethod* process_method;

MonoMethodDesc *ensure_init_app_method_desc;
MonoMethod *ensure_init_app_method;

//MonoClass * mono_header_class;
//MonoProperty * mono_header_key_prop;
//MonoProperty * mono_header_value_prop;

static void AddResponseHeader (ngx_http_request_t * r,MonoString * key,MonoString * value) 
	{
	    ngx_table_elt_t     *header;
	    
	    size_t              len;
	    
	    header = (ngx_table_elt_t*)(ngx_list_push(&r->headers_out.headers));
	    header->hash = 1;

	    len = mono_string_length(key);
	    header->key.len = len;
	    header->key.data = (u_char*)(ngx_pnalloc(r->pool, len));
	    ngx_memcpy(header->key.data, mono_string_to_utf8(key), len);

	    len = mono_string_length(value);
	    header->value.len = len;
	    header->value.data = (u_char*)(ngx_pnalloc(r->pool, len));
	    ngx_memcpy(header->value.data, mono_string_to_utf8(value), len);

	}

static MonoString * GetRequestBodyFileName(ngx_http_request_t * r)
 {
	if(r->request_body == NULL)
		ngx_log_error(NGX_LOG_ERR, r->connection->log, 0, "Request Body is Null");
	if(r->request_body->temp_file == NULL)
		ngx_log_error(NGX_LOG_ERR, r->connection->log, 0, "Temp File is Null");
	/*if(r->request_body->temp_file->file == NULL)
		ngx_log_error(NGX_LOG_ERR, r->connection->log, 0, "Temp File file is Null");
	if(r->request_body->temp_file->file.name == NULL)
		ngx_log_error(NGX_LOG_ERR, r->connection->log, 0, "Temp File file name is Null");
*/
 return mono_string_new_len (domain, (const char *)r->request_body->temp_file->file.name.data,r->request_body->temp_file->file.name.len);

}
static void WriteNginxLog(ngx_http_request_t * r,MonoString * message)
 {
		ngx_log_error(NGX_LOG_ERR, r->connection->log, 0, mono_string_to_utf8(message));
 
}
static void ReadClientBody (ngx_http_request_t * r,ngx_http_client_body_handler_pt function) 
	{
		r->request_body_in_file_only = 1;
        	r->request_body_in_persistent_file = 1;
        	r->request_body_in_clean_file = 1;
        	r->request_body_file_group_access = 1;
        	r->request_body_file_log_level = 0;
		ngx_http_read_client_request_body(r,function);
	}

static NginxMonoRequestInfo GetNginxMonoRequestInfo (ngx_http_request_t * r) 
	{
		NginxMonoRequestInfo st;
		st.method_name = mono_string_new_len (domain, (const char *)r->method_name.data,r->method_name.len);;
		st.http_protocol = mono_string_new_len (domain, (const char *)r->http_protocol.data,r->http_protocol.len);
		st.uri = NULL;
		st.args = NULL;
		if(r->uri.len != 0)
		{
		st.uri = mono_string_new_len (domain, (const char *)r->uri.data,r->uri.len);
		}
		if(r->args.len != 0)
		 {
		st.args = mono_string_new_len (domain, (const char *)r->args.data,r->args.len);
		} 
		ngx_uint_t                    i;
		int count;
		count = 0;
		ngx_list_part_t              *part;
		ngx_table_elt_t              *header;
		part = &r->headers_in.headers.part;
		header = part->elts;
		for (i = 0; /* void */; i++) {

		    if (i >= part->nelts) {
		        if (part->next == NULL) {
		            break;
		        }

		        part = part->next;
		        header = part->elts;
		        i = 0;
		    }

		    count++;
		}
		st.headers_count = count;
		//extract_mono_headers(r,&st);
		return st;
	}

static void addNginxHeader(ngx_list_part_t *part,NginxMonoHeader ** mheaders,MonoDomain * current_domain)
{
	ngx_uint_t i;
	ngx_table_elt_t * header;
	header = part->elts;
	for (i = 0;; i++) 
	{
		if (i >= part->nelts) 
		{
			break;
		}
		MonoString * args;
		args = mono_string_new_len (current_domain, (const char *)header[i].key.data,header[i].key.len);
		(*mheaders)->Key = args;
		args = mono_string_new_len (current_domain, (const char *)header[i].value.data,header[i].value.len);
		(*mheaders)->Value = args;
		(*mheaders)++;
	}
}


static void GetNginxHeaders (ngx_http_request_t * r, NginxMonoHeader ** passedmheaders,int count) 
{
	ngx_list_part_t              *part;
	ngx_table_elt_t              *header;
	ngx_uint_t                    i,key;
	//size_t                        len;
	 
	MonoDomain * current_domain = mono_domain_get();
		//MonoArray * array;   
	*passedmheaders = ((NginxMonoHeader*) g_malloc(sizeof(NginxMonoHeader) * 
	(count  * 2)
));
	NginxMonoHeader * mheaders;
	mheaders = *passedmheaders; //used to navigate.
		//array = mono_array_new(current_domain,mono_header_class,count);

	part = &r->headers_in.headers.part;
	header = part->elts;
	addNginxHeader(part,&mheaders,current_domain); //add Host header
	for (i = 0; /* void */; i++) 
	{
		if (i >= part->nelts) 
		{
			if (part->next == NULL) 
			{
		    	break;
		    }
		    part = part->next;
		    header = part->elts;
		    i = 0;
		}
		key = header[i].hash;
		//MonoObject * ex;
		//ex = NULL;
		//	MonoObject *header_instance;
		//header_instance = mono_object_new (current_domain, mono_header_class);
		MonoString * args;
		args = mono_string_new_len (current_domain, (const char *)header[i].key.data,header[i].key.len);
		mheaders->Key = args;
/*

			mono_property_set_value (
				mono_header_key_prop,header_instance 
				, (void*)&args, 
				&ex);
*/
		args = mono_string_new_len (current_domain, (const char *)header[i].value.data,header[i].value.len);
		mheaders->Value = args;
			/*mono_property_set_value (
				mono_header_value_prop,header_instance 
				, (void*)&args, 
				&ex);
*/
			//mono_array_setref (array, i, header_instance);
		mheaders++;
		
	}//for
}

static int 
	NginxWriteResponse (ngx_http_request_t * r,MonoArray * writtenDataArray,MonoString * contentTypeMono,int statusCode) 
	{

		u_int writtenDataArrayLength = mono_array_length(writtenDataArray);

		u_char * writtenData = malloc(sizeof(u_char) * mono_array_length(writtenDataArray));
u_int i;
		for( i =0;i < writtenDataArrayLength;i++)
		{
			writtenData[i] = mono_array_get(writtenDataArray,u_char,i);
		}

		//ngx_log_error(NGX_LOG_ERR, r->connection->log, 0, "Writting data");

		ngx_buf_t    *b;
		  ngx_chain_t   out;
 

		  b = ngx_pcalloc(r->pool, sizeof(ngx_buf_t));

		  out.buf = b;
		  out.next = NULL;
			
		  b->pos = writtenData;
		  b->last = writtenData + writtenDataArrayLength;
		  b->memory = 1;
		  b->last_buf = 1;

		  r->headers_out.status = statusCode;
		  r->headers_out.content_length_n = writtenDataArrayLength;
 
		u_char * content = (u_char*)mono_string_to_utf8(contentTypeMono);
		r->headers_out.content_type.len = mono_string_length(contentTypeMono);
		r->headers_out.content_type.data = (u_char *) content;		 

		ngx_http_send_header(r);
		  
		//ngx_log_error(NGX_LOG_ERR, r->connection->log, 0, "Buffer Written");
		int result = ngx_http_output_filter(r, &out);
g_free(writtenData); //free the unboxed data from the managed world.
return result;
	}
//#define X_SECURE
#if X_SECURE
static gboolean
determinate_if_platform_assembly_callback (const char *image_name)
{
	 fprintf(stderr,"Platform Assembly: %s\n",image_name);
	return TRUE;
}
#endif
void ensure_mono(ngx_log_t  *log)
{   
	ngx_log_error(NGX_LOG_ERR, log, 0, "Ensuring Mono");    
	if(domain) return;  
	#if X_SECURE
	mono_security_set_core_clr_platform_callback(determinate_if_platform_assembly_callback);
	mono_security_enable_core_clr ();
	#endif         
	ngx_log_error(NGX_LOG_ERR, log, 0, "Initializing Mono Domain");
	domain = mono_jit_init_version ("MonoNginxDomain","v2.0.50727");
	mono_config_parse (NULL);
	mono_add_internal_call ("MainApp::NginxWriteResponse", NginxWriteResponse);
	mono_add_internal_call ("MainApp::GetNginxMonoRequestInfo", GetNginxMonoRequestInfo);
	mono_add_internal_call ("MainApp::GetNginxHeaders", GetNginxHeaders);
	mono_add_internal_call ("MainApp::AddResponseHeader", AddResponseHeader);
	mono_add_internal_call ("MainApp::ReadClientBody", ReadClientBody);
	mono_add_internal_call ("MainApp::GetRequestBodyFileName", GetRequestBodyFileName);
	mono_add_internal_call ("MainApp::SecureEnvironment", SecureEnvironment);
	mono_add_internal_call ("MainApp::GetConfigurationAppPathDirectory", GetConfigurationAppPathDirectory);
	mono_add_internal_call ("MainApp::WriteNginxLog", WriteNginxLog);

	if(domain)
	{
		ngx_log_error(NGX_LOG_ERR, log, 0, "Domain Intialized");
	}else{
		ngx_log_error(NGX_LOG_ERR, log, 0, "Domain can not be initialized");
	}
 
	assembly = mono_domain_assembly_open (domain, "/home/thepumpkin/BlackLinks/nginx-local/sbin/NginxBlackLinks.dll");
if(assembly)
{	ngx_log_error(NGX_LOG_ERR, log, 0, "Assembly Loaded");
}else
{	ngx_log_error(NGX_LOG_ERR, log, 0, "Assembly  Could not be Loaded");
}
	if (assembly)
	{
		assembly_image = mono_assembly_get_image(assembly);
		myclass= mono_class_from_name (assembly_image, "", "MainApp");
		//mono_header_class = mono_class_from_name (assembly_image, "", "NginxMonoHeader");
		//mono_header_key_prop = mono_class_get_property_from_name(mono_header_class,"Key");
		//mono_header_value_prop = mono_class_get_property_from_name(mono_header_class,"Value");

		if(myclass)
		{	ngx_log_error(NGX_LOG_ERR, log, 0, "Class Loaded");
		}else{
			ngx_log_error(NGX_LOG_ERR, log, 0, "Class could not be Loaded");
		}
  		my_class_instance = mono_object_new (domain, myclass);
		if(my_class_instance)
		{	ngx_log_error(NGX_LOG_ERR, log, 0, "Class Instantiated");
		}else{
			ngx_log_error(NGX_LOG_ERR, log, 0, "Class could not been instantiated");
		}
  		mono_runtime_object_init (my_class_instance);

		process_method_desc = mono_method_desc_new("MainApp:Process(intptr)", TRUE);
		if(process_method_desc)
		{	ngx_log_error(NGX_LOG_ERR, log, 0, "Process Method Desc Found");
		}else{
			ngx_log_error(NGX_LOG_ERR, log, 0, "Process Method Desc NOT Found");
		}

		process_method = mono_method_desc_search_in_class(process_method_desc,myclass);
		if(process_method)
		{	ngx_log_error(NGX_LOG_ERR, log, 0, "Process Method Found");
		}else{
			ngx_log_error(NGX_LOG_ERR, log, 0, "Process Method NOT Found");
		}
		
		ensure_init_app_method_desc = mono_method_desc_new("MainApp:EnsureInitializeApp()", TRUE);
		if(ensure_init_app_method_desc)
		{	ngx_log_error(NGX_LOG_ERR, log, 0, "EnsureInitApp Method Desc Found");
		}else{
			ngx_log_error(NGX_LOG_ERR, log, 0, "EnsureInitApp Method Desc NOT Found");
		}
		ensure_init_app_method = mono_method_desc_search_in_class(ensure_init_app_method_desc,myclass);
		if(ensure_init_app_method)
		{	ngx_log_error(NGX_LOG_ERR, log, 0, "EnsureInitApp Method Found");
		}else{
			ngx_log_error(NGX_LOG_ERR, log, 0, "EnsureInitApp Method NOT Found");
		}
		mono_thread_attach(domain);
		 mono_runtime_invoke (ensure_init_app_method, my_class_instance, NULL, NULL);
	}
}

static ngx_int_t ngx_http_hello_world_handler(ngx_http_request_t *r)
{
	
	ngx_log_error(NGX_LOG_ERR, r->connection->log, 0, "ngx_http_hello_world_handler");
	//ngx_http_hello_world_loc_conf_t  *plcf;
	ngx_http_blacklinks_loc_conf_t *blcf;
	blcf = ngx_http_get_module_loc_conf(r, ngx_http_hello_world_module);
	//ngx_log_error(NGX_LOG_ERR, r->connection->log, 0, "Hello World Handler Requested");
	ensure_mono(r->connection->log); 
	
	//
	MonoThread * callt = mono_thread_attach(domain);
	  	void *args [1];
  	args [0] = &r; 
//ngx_log_error(NGX_LOG_ERR, r->connection->log, 0, ".NET Method Invoking");
  MonoObject * returnValueMono = mono_runtime_invoke (process_method, my_class_instance, args, NULL);
//ngx_log_error(NGX_LOG_ERR, r->connection->log, 0, ".NET Method Invoked");

	ngx_int_t res = *(int*)mono_object_unbox (returnValueMono);
	mono_thread_detach(callt);
  return res;
  
//return NGX_OK;
}

void *
ngx_blacklinks_create_loc_conf(ngx_conf_t *cf)
{
    ngx_http_blacklinks_loc_conf_t  *conf;
    conf = ngx_pcalloc(cf->pool, sizeof(ngx_http_blacklinks_loc_conf_t));
    if (conf == NULL) {
	    ngx_log_error(NGX_LOG_ERR, cf->log, 0, "We had an error initializing the configuration structure");
        return NULL;
    }
    ngx_log_error(NGX_LOG_ERR, cf->log, 0, "Configuration Structure Successfuly initializede");
    return conf;
}

static char *ngx_http_blacklinks_application_configuration(ngx_conf_t *cf, ngx_command_t *cmd, void *conf)
{
	
  ngx_http_core_loc_conf_t  *clcf;
  clcf = ngx_http_conf_get_module_loc_conf(cf, ngx_http_core_module);
  clcf->handler = ngx_http_hello_world_handler;
  
  ngx_http_blacklinks_loc_conf_t * bc = conf;
  if(!bc)
  {
	  ngx_log_error(NGX_LOG_ERR, cf->log, 0, "Invalid configuration structure");
	  return NGX_CONF_ERROR;
}
   
  if(cf->args->nelts > 0)
  {
  ngx_str_t                         *value = cf->args->elts;
  bc->blacklinks_app_dir.data = value[1].data;
  bc->blacklinks_app_dir.len = value[1].len;
  ngx_log_error(NGX_LOG_ERR, cf->log, 0, (const char *)bc->blacklinks_app_dir.data);
  }
  global_blacklinks_conf = bc;
  
  // Now that we have the configuration, Initialize the app inmediately.
  //ensure_mono(cf);  
  return NGX_CONF_OK;
}

void SecureEnvironment()
{
	 
}

MonoString* GetConfigurationAppPathDirectory() {
	return mono_string_new_len (domain, (const char *)global_blacklinks_conf->blacklinks_app_dir.data,global_blacklinks_conf->blacklinks_app_dir.len);
}
